#nullable disable
using Microsoft.EntityFrameworkCore;
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Db;

public static class QuoteAliasesExtensions
{
    public static IEnumerable<QuoteAliases> GetForGuild(this DbSet<QuoteAliases> quoteAliases, ulong guildId)
        => quoteAliases.AsQueryable().Where(x => x.GuildId == guildId);

    public static async Task<QuoteAliases> GetKeywordForAlias(
        this DbSet<QuoteAliases> quoteAliases,
        ulong guildId,
        string alias)
    {
        return (await quoteAliases.AsQueryable().Where(q => q.GuildId == guildId && q.Alias == alias).ToListAsync())
            .FirstOrDefault();
    }

    public static async Task<List<QuoteAliases>> GetAllAliasesForKeyword(
        this DbSet<QuoteAliases> quoteAliases,
        ulong guildId,
        string keyword)
    {
        return (await quoteAliases.AsQueryable().Where(q => q.GuildId == guildId && q.ReferencedKeyword == keyword).ToListAsync());
    }
}