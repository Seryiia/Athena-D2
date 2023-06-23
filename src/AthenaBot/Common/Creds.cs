#nullable disable
using AthenaBot.Common.Yml;

namespace AthenaBot.Common;

public sealed class Creds : IBotCredentials
{
    [Comment(@"DO NOT CHANGE")]
    public int Version { get; set; }

    [Comment(@"Bot token. Do not share with anyone ever -> https://discordapp.com/developers/applications/")]
    public string Token { get; set; }

    [Comment(@"List of Ids of the users who have bot owner permissions
**DO NOT ADD PEOPLE YOU DON'T TRUST**")]
    public ICollection<ulong> OwnerIds { get; set; }
    
    [Comment(@"The number of shards that the bot will be running on.
Leave at 1 if you don't know what you're doing.

note: If you are planning to have more than one shard, then you must change botCache to 'redis'.
      Also, in that case you should be using AthenaBot.Coordinator to start the bot, and it will correctly override this value.")]
    public int TotalShards { get; set; }
    
    [Comment(@"Which cache implementation should bot use.
'memory' - Cache will be in memory of the bot's process itself. Only use this on bots with a single shard. When the bot is restarted the cache is reset. 
'redis' - Uses redis (which needs to be separately downloaded and installed). The cache will persist through bot restarts. You can configure connection string in creds.yml")]
    public BotCacheImplemenation BotCache { get; set; }
    
    [Comment(@"Redis connection string. Don't change if you don't know what you're doing.
Only used if botCache is set to 'redis'")]
    public string RedisOptions { get; set; }

    [Comment(@"Database options. Don't change if you don't know what you're doing. Leave null for default values")]
    public DbOptions Db { get; set; }

    [Comment(@"Address and port of the coordinator endpoint. Leave empty for default.
Change only if you've changed the coordinator address or port.")]
    public string CoordinatorUrl { get; set; }

    [Comment(@"Obtain by creating an application at https://dev.twitch.tv/console/apps")]
    public string TwitchClientId { get; set; }

    [Comment(@"Obtain by creating an application at https://dev.twitch.tv/console/apps")]
    public string TwitchClientSecret { get; set; }

    [Comment(@"Command and args which will be used to restart the bot.
Only used if bot is executed directly (NOT through the coordinator)
placeholders: 
    {0} -> shard id 
    {1} -> total shards
Linux default
    cmd: dotnet
    args: ""AthenaBot.dll -- {0}""
Windows default
    cmd: AthenaBot.exe
    args: ""{0}""")]
    public RestartConfig RestartCommand { get; set; }

    public Creds()
    {
        Version = 7;
        Token = string.Empty;
        OwnerIds = new List<ulong>();
        TotalShards = 1;
        BotCache = BotCacheImplemenation.Memory;
        RedisOptions = "localhost:6379,syncTimeout=30000,responseTimeout=30000,allowAdmin=true,password=";
        Db = new()
        {
            Type = "sqlite",
            ConnectionString = "Data Source=data/AthenaBot.db"
        };

        CoordinatorUrl = "http://localhost:3442";

        RestartCommand = new();
    }


    public class DbOptions
    {
        [Comment(@"Database type. ""sqlite"", ""mysql"" and ""postgresql"" are supported.
Default is ""sqlite""")]
        public string Type { get; set; }

        [Comment(@"Database connection string.
You MUST change this if you're not using ""sqlite"" type.
Default is ""Data Source=data/AthenaBot.db""
Example for mysql: ""Server=localhost;Port=3306;Uid=root;Pwd=my_super_secret_mysql_password;Database=athena""
Example for postgresql: ""Server=localhost;Port=5432;User Id=postgres;Password=my_super_secret_postgres_password;Database=athena;""")]
        public string ConnectionString { get; set; }
    }

    public sealed record PatreonSettings
    {
        public string ClientId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ClientSecret { get; set; }

        [Comment(
            @"Campaign ID of your patreon page. Go to your patreon page (make sure you're logged in) and type ""prompt('Campaign ID', window.patreon.bootstrap.creator.data.id);"" in the console. (ctrl + shift + i)")]
        public string CampaignId { get; set; }

        public PatreonSettings(
            string accessToken,
            string refreshToken,
            string clientSecret,
            string campaignId)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ClientSecret = clientSecret;
            CampaignId = campaignId;
        }

        public PatreonSettings()
        {
        }
    }

    public sealed record VotesSettings
    {
        [Comment(@"top.gg votes service url
This is the url of your instance of the AthenaBot.Votes api
Example: https://votes.my.cool.bot.com")]
        public string TopggServiceUrl { get; set; }

        [Comment(@"Authorization header value sent to the TopGG service url with each request
This should be equivalent to the TopggKey in your AthenaBot.Votes api appsettings.json file")]
        public string TopggKey { get; set; }

        [Comment(@"discords.com votes service url
This is the url of your instance of the AthenaBot.Votes api
Example: https://votes.my.cool.bot.com")]
        public string DiscordsServiceUrl { get; set; }

        [Comment(@"Authorization header value sent to the Discords service url with each request
This should be equivalent to the DiscordsKey in your AthenaBot.Votes api appsettings.json file")]
        public string DiscordsKey { get; set; }

        public VotesSettings()
        {
        }

        public VotesSettings(
            string topggServiceUrl,
            string topggKey,
            string discordsServiceUrl,
            string discordsKey)
        {
            TopggServiceUrl = topggServiceUrl;
            TopggKey = topggKey;
            DiscordsServiceUrl = discordsServiceUrl;
            DiscordsKey = discordsKey;
        }
    }
}

public class GoogleApiConfig
{
    public string SearchId { get; init; }
    public string ImageSearchId { get; init; }
}

public enum BotCacheImplemenation
{
    Memory,
    Redis
}