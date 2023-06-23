#nullable disable
using AthenaBot.Db.Models;

namespace AthenaBot.Services.Database.Models;

public class GuildConfig : DbEntity
{
    public ulong GuildId { get; set; }

    public string Prefix { get; set; }

    public bool DeleteMessageOnCommand { get; set; }
    public HashSet<DelMsgOnCmdChannel> DelMsgOnCmdChannels { get; set; } = new();

    //stream notifications
    public HashSet<FollowedStream> FollowedStreams { get; set; } = new();

    //public bool FilterLinks { get; set; }
    //public HashSet<FilterLinksChannelId> FilterLinksChannels { get; set; } = new HashSet<FilterLinksChannelId>();

    public bool FilterWords { get; set; }
    public HashSet<FilteredWord> FilteredWords { get; set; } = new();

    public string Locale { get; set; }
    public string TimeZoneId { get; set; }
    public bool VerboseErrors { get; set; } = true;
    public List<FeedSub> FeedSubs { get; set; } = new();
    public bool NotifyStreamOffline { get; set; }
    public bool DeleteStreamOnlineMessage { get; set; }

    public bool DisableGlobalExpressions { get; set; } = false;
}