#nullable disable
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Db.Models;

public class Birthday : DbEntity
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }
    public ulong ChannelId { get; set; }
    public ushort Month { get; set; }
    public ushort Day { get; set; }
}