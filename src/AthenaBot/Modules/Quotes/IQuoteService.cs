namespace AthenaBot.Modules.Quotes;

public interface IQuoteService
{
    Task<int> DeleteAllAuthorQuotesAsync(ulong guildId, ulong userId);
}