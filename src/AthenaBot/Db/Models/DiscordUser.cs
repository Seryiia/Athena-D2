#nullable disable
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Db.Models;


// FUTURE remove LastLevelUp from here and UserXpStats
public class DiscordUser : DbEntity
{
    public ulong UserId { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string AvatarId { get; set; }

    public override bool Equals(object obj)
        => obj is DiscordUser du ? du.UserId == UserId : false;

    public override int GetHashCode()
        => UserId.GetHashCode();

    public override string ToString()
        => Username + "#" + Discriminator;
}