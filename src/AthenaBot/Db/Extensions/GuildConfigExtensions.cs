#nullable disable
using Microsoft.EntityFrameworkCore;
using AthenaBot.Db.Models;
using AthenaBot.Services.Database;
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Db;

public static class GuildConfigExtensions
{
    private static IQueryable<GuildConfig> IncludeEverything(this DbSet<GuildConfig> configs)
        => configs.AsQueryable()
                  .AsSplitQuery()
                  .Include(gc => gc.FollowedStreams)
                  .Include(gc => gc.DelMsgOnCmdChannels)
                  .Include(gc => gc.VanityRoles);

    public static IEnumerable<GuildConfig> GetAllGuildConfigs(
        this DbSet<GuildConfig> configs,
        List<ulong> availableGuilds)
        => configs.IncludeEverything().AsNoTracking().Where(x => availableGuilds.Contains(x.GuildId)).ToList();

    /// <summary>
    ///     Gets and creates if it doesn't exist a config for a guild.
    /// </summary>
    /// <param name="ctx">Context</param>
    /// <param name="guildId">Id of the guide</param>
    /// <param name="includes">Use to manipulate the set however you want. Pass null to include everything</param>
    /// <returns>Config for the guild</returns>
    public static GuildConfig GuildConfigsForId(
        this AthenaContext ctx,
        ulong guildId,
        Func<DbSet<GuildConfig>, IQueryable<GuildConfig>> includes)
    {
        GuildConfig config;

        if (includes is null)
            config = ctx.GuildConfigs.IncludeEverything().FirstOrDefault(c => c.GuildId == guildId);
        else
        {
            var set = includes(ctx.GuildConfigs);
            config = set.FirstOrDefault(c => c.GuildId == guildId);
        }

        if (config is null)
        {
            ctx.GuildConfigs.Add(config = new()
            {
                GuildId = guildId,
            });
            ctx.SaveChanges();
        }

        return config;

        // ctx.GuildConfigs
        //    .ToLinqToDBTable()
        //    .InsertOrUpdate(() => new()
        //        {
        //            GuildId = guildId,
        //            Permissions = Permissionv2.GetDefaultPermlist,
        //            WarningsInitialized = true,
        //            WarnPunishments = DefaultWarnPunishments
        //        },
        //        _ => new(),
        //        () => new()
        //        {
        //            GuildId = guildId
        //        });
        //
        // if(includes is null)
        // return ctx.GuildConfigs
        //    .ToLinqToDBTable()
        //    .First(x => x.GuildId == guildId);
    }

    public static IEnumerable<FollowedStream> GetFollowedStreams(this DbSet<GuildConfig> configs)
        => configs.AsQueryable().Include(x => x.FollowedStreams).SelectMany(gc => gc.FollowedStreams).ToArray();

    public static IEnumerable<FollowedStream> GetFollowedStreams(this DbSet<GuildConfig> configs, List<ulong> included)
        => configs.AsQueryable()
                  .Where(gc => included.Contains(gc.GuildId))
                  .Include(gc => gc.FollowedStreams)
                  .SelectMany(gc => gc.FollowedStreams)
                  .ToList();

    public class GeneratingChannel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}