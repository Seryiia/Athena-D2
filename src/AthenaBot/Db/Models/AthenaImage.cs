#nullable disable
using AthenaBot.Modules.StreamNotification.Models;
using AthenaBot.Services.Database.Models;

namespace AthenaBot.Db.Models;

public class AthenaImage : DbEntity
{
    public ulong GuildId { get; set; }
    public string Name { get; set; }

    public string Extension { get; set; }
    public byte[] Image { get; set; }

    protected bool Equals(AthenaImage other)
        => Name.Trim().ToUpperInvariant() == other.Name.Trim().ToUpperInvariant()
           && Image == other.Image;

    public override int GetHashCode()
        => HashCode.Combine(Name, Image);

    public override bool Equals(object obj)
        => obj is Image image && Equals(image);
}