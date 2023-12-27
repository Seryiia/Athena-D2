#nullable disable
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using AthenaBot.Db.Models;
using AthenaBot.Services.Database;

namespace AthenaBot.Db;

public static class DiscordUserExtensions
{
    public static Task<DiscordUser> GetByUserIdAsync(
        this IQueryable<DiscordUser> set,
        ulong userId)
        => set.FirstOrDefaultAsyncLinqToDB(x => x.UserId == userId);
    
    public static void EnsureUserCreated(
        this AthenaContext ctx,
        ulong userId,
        string username,
        string discrim,
        string avatarId)
        => ctx.DiscordUser.ToLinqToDBTable()
              .InsertOrUpdate(
                  () => new()
                  {
                      UserId = userId,
                      Username = username,
                      Discriminator = discrim,
                      AvatarId = avatarId
                  },
                  old => new()
                  {
                      Username = username,
                      Discriminator = discrim,
                      AvatarId = avatarId
                  },
                  () => new()
                  {
                      UserId = userId
                  });

    public static Task EnsureUserCreatedAsync(
        this AthenaContext ctx,
        ulong userId)
        => ctx.DiscordUser
              .ToLinqToDBTable()
              .InsertOrUpdateAsync(
                  () => new()
                  {
                      UserId = userId,
                      Username = "Unknown",
                      Discriminator = "????",
                      AvatarId = string.Empty
                  },
                  old => new()
                  {

                  },
                  () => new()
                  {
                      UserId = userId
                  });
    
    //temp is only used in updatecurrencystate, so that i don't overwrite real usernames/discrims with Unknown
    public static DiscordUser GetOrCreateUser(
        this AthenaContext ctx,
        ulong userId,
        string username,
        string discrim,
        string avatarId,
        Func<IQueryable<DiscordUser>, IQueryable<DiscordUser>> includes = null)
    {
        ctx.EnsureUserCreated(userId, username, discrim, avatarId);

        IQueryable<DiscordUser> queryable = ctx.DiscordUser;
        if (includes is not null)
            queryable = includes(queryable);
        return queryable.First(u => u.UserId == userId);
    }

    public static DiscordUser GetOrCreateUser(this AthenaContext ctx, IUser original, Func<IQueryable<DiscordUser>, IQueryable<DiscordUser>> includes = null)
        => ctx.GetOrCreateUser(original.Id, original.Username, original.Discriminator, original.AvatarId, includes);
}