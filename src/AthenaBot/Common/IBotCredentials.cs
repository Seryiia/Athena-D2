#nullable disable
namespace AthenaBot;

public interface IBotCredentials
{
    string Token { get; }
    ICollection<ulong> OwnerIds { get; }
    Creds.DbOptions Db { get; }
    int TotalShards { get; }
    RestartConfig RestartCommand { get; }
    string RedisOptions { get; }
    string CoordinatorUrl { get; set; }
    string TwitchClientId { get; set; }
    string TwitchClientSecret { get; set; }
    BotCacheImplemenation BotCache { get; set; }
}

public class RestartConfig
{
    public string Cmd { get; set; }
    public string Args { get; set; }
}