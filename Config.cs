using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AutomaticAds;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = " [{GREEN}AutomaticAds{WHITE}]{WHITE}";

    [JsonPropertyName("PlaySoundName")]
    public string? PlaySoundName { get; set; } = "ui/panorama/popup_reveal_01";

    [JsonPropertyName("Ads")]
    public List<AdConfig> Ads { get; set; } = new()
    {
        new AdConfig
        {
            Message = "{RED}AutomaticAds is the best plugin!",
            Map = "all",
            Interval = 600,
            DisableSound = false,
            Flag = "all"
        },
        new AdConfig
        {
            Message = "{BLUE}Welcome to the server! {RED}Make sure to read the rules.",
            Map = "all",
            Interval = 800,
            DisableSound = false,
            Flag = "all"
        },
        new AdConfig
        {
            Message = "{BLUE}Thank you for supporting the server! {GOLD}Your contribution is greatly appreciated.",
            Map = "all",
            Interval = 1000,
            DisableSound = true,
            Flag = "@css/vip"
        },
        new AdConfig
        {
            Message = "{GOLD}Congratulations, you are playing on Mirage.",
            Map = "de_mirage",
            Interval = 1400,
            DisableSound = true,
            Flag = "all"
        }
    };

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