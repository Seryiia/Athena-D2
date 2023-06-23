#nullable disable
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using AthenaBot.Common.ModuleBehaviors;
using AthenaBot.Db;
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Modules.Administration;

public sealed class FilterService : IExecOnMessage
{
    //serverid, filteredwords
    public ConcurrentDictionary<ulong, ConcurrentHashSet<string>> ServerFilteredWords { get; }
    public ConcurrentHashSet<ulong> WordFilteringServers { get; }


    public int Priority
        => int.MaxValue - 1;

    private readonly DbService _db;

    public FilterService(DiscordSocketClient client, DbService db)
    {
        _db = db;

        using (var uow = db.GetDbContext())
        {
            var ids = client.GetGuildIds();
            var configs = uow.Set<GuildConfig>()
                             .AsQueryable()
                             .Include(x => x.FilteredWords)
                             .Where(gc => ids.Contains(gc.GuildId))
                             .ToList();

            var dict = configs.ToDictionary(gc => gc.GuildId,
                gc => new ConcurrentHashSet<string>(gc.FilteredWords.Select(fw => fw.Word).Distinct()));

            ServerFilteredWords = new(dict);

            var serverFiltering = configs.Where(gc => gc.FilterWords);
            WordFilteringServers = new(serverFiltering.Select(gc => gc.GuildId));
        }

        client.MessageUpdated += (oldData, newMsg, channel) =>
        {
            _ = Task.Run(() =>
            {
                var guild = (channel as ITextChannel)?.Guild;

                if (guild is null || newMsg is not IUserMessage usrMsg)
                    return Task.CompletedTask;

                return ExecOnMessageAsync(guild, usrMsg);
            });
            return Task.CompletedTask;
        };
    }

    public void ClearFilteredWords(ulong guildId)
    {
        using var uow = _db.GetDbContext();
        var gc = uow.GuildConfigsForId(guildId,
            set => set.Include(x => x.FilteredWords));

        WordFilteringServers.TryRemove(guildId);
        ServerFilteredWords.TryRemove(guildId, out _);

        gc.FilterWords = false;
        gc.FilteredWords.Clear();

        uow.SaveChanges();
    }

    public ConcurrentHashSet<string> FilteredWordsForServer(ulong guildId)
    {
        var words = new ConcurrentHashSet<string>();
        if (WordFilteringServers.Contains(guildId))
            ServerFilteredWords.TryGetValue(guildId, out words);
        return words;
    }

    public async Task<bool> ExecOnMessageAsync(IGuild guild, IUserMessage msg)
    {
        if (msg.Author is not IGuildUser gu || gu.GuildPermissions.Administrator)
            return false;

        var results = await FilterWords(guild, msg);

        return results;
    }

    private async Task<bool> FilterWords(IGuild guild, IUserMessage usrMsg)
    {
        if (guild is null)
            return false;
        if (usrMsg is null)
            return false;

        var filteredServerWords = FilteredWordsForServer(guild.Id) ?? new ConcurrentHashSet<string>();
        var wordsInMessage = usrMsg.Content.ToLowerInvariant().Split(' ');
        if (filteredServerWords.Count != 0)
        {
            foreach (var word in wordsInMessage)
            {
                if (filteredServerWords.Contains(word))
                {
                    Log.Information("User {UserName} [{UserId}] used a filtered word in {ChannelId} channel",
                        usrMsg.Author.ToString(),
                        usrMsg.Author.Id,
                        usrMsg.Channel.Id);

                    try
                    {
                        await usrMsg.DeleteAsync();
                    }
                    catch (HttpException ex)
                    {
                        Log.Warning(ex,
                            "I do not have permission to filter words in channel with id {Id}",
                            usrMsg.Channel.Id);
                    }

                    return true;
                }
            }
        }

        return false;
    }
}