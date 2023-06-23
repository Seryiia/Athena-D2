#nullable disable
using AthenaBot.Common.ModuleBehaviors;
using System.Collections.Immutable;

namespace AthenaBot.Modules.Administration.Services;

public sealed class BotManagement : IReadyExecutor, INService
{
    private readonly DbService _db;
    private readonly DiscordSocketClient _client;

    private readonly IBotCredentials _creds;

    private ImmutableDictionary<ulong, IDMChannel> ownerChannels =
        new Dictionary<ulong, IDMChannel>().ToImmutableDictionary();


    private readonly IHttpClientFactory _httpFactory;
    private readonly IPubSub _pubSub;

    //keys
    private readonly TypedKey<ActivityPubData> _activitySetKey;
    private readonly TypedKey<string> _guildLeaveKey;

    public BotManagement(
        DiscordSocketClient client,
        DbService db,
        IBotCredentials creds,
        IHttpClientFactory factory,
        IPubSub pubSub)
    {
        _db = db;
        _client = client;
        _creds = creds;
        _httpFactory = factory;
        _pubSub = pubSub;
        _activitySetKey = new("activity.set");
        _guildLeaveKey = new("guild.leave");

        HandleStatusChanges();

        _pubSub.Sub(_guildLeaveKey,
            async input =>
            {
                var guildStr = input.ToString().Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(guildStr))
                    return;

                var server = _client.Guilds.FirstOrDefault(g => g.Id.ToString() == guildStr
                                                                || g.Name.Trim().ToUpperInvariant() == guildStr);
                if (server is null)
                    return;

                if (server.OwnerId != _client.CurrentUser.Id)
                {
                    await server.LeaveAsync();
                    Log.Information("Left server {Name} [{Id}]", server.Name, server.Id);
                }
                else
                {
                    await server.DeleteAsync();
                    Log.Information("Deleted server {Name} [{Id}]", server.Name, server.Id);
                }
            });
    }

    public async Task OnReadyAsync()
    {
        await using var uow = _db.GetDbContext();

        if (_client.ShardId == 0)
            await LoadOwnerChannels();
    }

    private async Task LoadOwnerChannels()
    {
        var channels = await _creds.OwnerIds.Select(id =>
            {
                var user = _client.GetUser(id);
                if (user is null)
                    return Task.FromResult<IDMChannel>(null);

                return user.CreateDMChannelAsync();
            })
            .WhenAll();

        ownerChannels = channels.Where(x => x is not null)
            .ToDictionary(x => x.Recipient.Id, x => x)
            .ToImmutableDictionary();

        if (!ownerChannels.Any())
        {
            Log.Warning(
                "No owner channels created! Make sure you've specified the correct OwnerId in the creds.yml file and invited the bot to a Discord server");
        }
        else
        {
            Log.Information("Created {OwnerChannelCount} out of {TotalOwnerChannelCount} owner message channels",
                ownerChannels.Count,
                _creds.OwnerIds.Count);
        }
    }

    public Task LeaveGuild(string guildStr)
        => _pubSub.Pub(_guildLeaveKey, guildStr);

    public async Task<bool> SetAvatar(string img)
    {
        if (string.IsNullOrWhiteSpace(img))
            return false;

        if (!Uri.IsWellFormedUriString(img, UriKind.Absolute))
            return false;

        var uri = new Uri(img);

        using var http = _httpFactory.CreateClient();
        using var sr = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        if (!sr.IsImage())
            return false;

        // i can't just do ReadAsStreamAsync because dicord.net's image poops itself
        var imgData = await sr.Content.ReadAsByteArrayAsync();
        await using var imgStream = imgData.ToStream();
        await _client.CurrentUser.ModifyAsync(u => u.Avatar = new Image(imgStream));

        return true;
    }

    private void HandleStatusChanges()
        => _pubSub.Sub(_activitySetKey,
            async data =>
            {
                try
                {
                    await _client.SetGameAsync(data.Name, data.Link, data.Type);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error setting activity");
                }
            });

    public Task SetGameAsync(string game, ActivityType type)
        => _pubSub.Pub(_activitySetKey,
            new()
            {
                Name = game,
                Link = null,
                Type = type
            });

    public Task SetStreamAsync(string name, string link)
        => _pubSub.Pub(_activitySetKey,
            new()
            {
                Name = name,
                Link = link,
                Type = ActivityType.Streaming
            });

    private sealed class ActivityPubData
    {
        public string Name { get; init; }
        public string Link { get; init; }
        public ActivityType Type { get; init; }
    }
}