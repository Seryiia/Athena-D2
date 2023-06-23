#nullable disable
using CommandLine;

namespace AthenaBot.Common;

public class LbOpts : IAthenaCommandOptions
{
    [Option('c', "clean", Default = false, HelpText = "Only show users who are on the server.")]
    public bool Clean { get; set; }

    public void NormalizeOptions()
    {
    }
}