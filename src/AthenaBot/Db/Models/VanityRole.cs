#nullable disable
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Db.Models;

public class VanityRole : DbEntity
{
    public enum VanityRoleType
    {
        Birthday = 0,
        Peach = 1,
        BapToSleep = 2
    }

    public ulong GuildId { get; set; }
    public VanityRoleType Type { get; set; }
    public ulong RoleId { get; set; }

    protected bool Equals(VanityRole other)
        => RoleId == other.RoleId
           && Type == other.Type;

    public override int GetHashCode()
        => HashCode.Combine(RoleId, (int)Type);

    public override bool Equals(object obj)
        => obj is VanityRole fs && Equals(fs);
}