#nullable disable
using System.ComponentModel.DataAnnotations;

namespace AthenaBot.Services.Database.Models;

public class QuoteAliases : DbEntity
{
    public ulong GuildId { get; set; }

    [Required]
    public string Alias { get; set; }

    [Required]
    public string ReferencedKeyword { get; set; }
}