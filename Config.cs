using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AutomaticAds;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = " [{GREEN}AutomaticAds]";

    [JsonPropertyName("PlaySoundName")]
    public string? PlaySoundName { get; set; } = "ui/panorama/popup_reveal_01";


    [JsonPropertyName("Ads")]
    public List<AdConfig> Ads { get; set; } = new();

    public class AdConfig
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("flag")]
        public string? Flag { get; set; } = null;

        [JsonPropertyName("map")]
        public string Map { get; set; } = "all";

        [JsonPropertyName("interval")]
        public float Interval { get; set; } = 600;

        [JsonPropertyName("disableSound")]
        public bool DisableSound { get; set; } = false;
    }
}