#nullable disable
using Newtonsoft.Json;

namespace AthenaBot;

public class SmartTextEmbedFooter
{
    public string Text { get; set; }

    [JsonProperty("icon_url")]
    public string IconUrl { get; set; }
}