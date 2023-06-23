#nullable disable
using AthenaBot;
using AthenaBot.Db.Models;

namespace AthenaBot.Modules.StreamNotification.Models;

public readonly struct StreamDataKey
{
    public FollowedStream.FType Type { get; init; }
    public string Name { get; init; }

    public StreamDataKey(FollowedStream.FType type, string name)
    {
        Type = type;
        Name = name;
    }
}