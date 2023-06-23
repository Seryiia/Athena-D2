#nullable disable
using LinqToDB;
using Microsoft.EntityFrameworkCore;
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Db;

public static class AthenaExpressionExtensions
{
    public static int ClearFromGuild(this DbSet<AthenaExpression> exprs, ulong guildId)
        => exprs.Delete(x => x.GuildId == guildId);

    public static IEnumerable<AthenaExpression> ForId(this DbSet<AthenaExpression> exprs, ulong id)
        => exprs.AsNoTracking().AsQueryable().Where(x => x.GuildId == id).ToList();
}