using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AutomaticAds;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = " [{GREEN}AutomaticAds{WHITE}]{WHITE}";

    [JsonPropertyName("PlaySoundName")]
    public string? PlaySoundName { get; set; } = "ui/panorama/popup_reveal_01";

    [JsonPropertyName("sendAdsInOrder")]
    public bool SendAdsInOrder { get; set; } = true;

    [JsonPropertyName("UseWelcomeMessage")]
    public bool EnableWelcomeMessage { get; set; } = true;

    [JsonPropertyName("Welcome")]
    public List<WelcomeConfig> Welcome { get; set; } = new()
    {
        new WelcomeConfig
        {
            WelcomeMessage = "{BLUE}Welcome to the server {playername}! {RED}Playing on {map} with {players}/{maxplayers} players.",
            ViewFlag = "all",
            ExcludeFlag = "",
            DisableSound = false
        }
    };

    [JsonPropertyName("Ads")]
    public List<AdConfig> Ads { get; set; } = new()
    {
        new AdConfig
        {
            Message = "{RED}AutomaticAds is the best plugin!",
            Map = "all",
            Interval = 600,
            DisableSound = false,
            ViewFlag = "all"
        },
        new AdConfig
        {
            Message = "{BLUE}Welcome to {hostname}! {RED}The time is {time} of {date}, playing in {map} with {players}/{maxplayers}. Connect {ip}",
            Map = "all",
            Interval = 800,
            DisableSound = false,
            ViewFlag = "all"
        },
        new AdConfig
        {
            Message = "{BLUE}Thank you for supporting the server! {GOLD}Your contribution is greatly appreciated.",
            Map = "all",
            Interval = 1000,
            DisableSound = true,
            ViewFlag = "@css/vip"
        },
        new AdConfig
        {
            Message = "{GOLD}Congratulations {playername}, you are playing on Mirage.",
            ExcludeFlag = "@css/vip",
            Map = "de_mirage",
            Interval = 1400,
            DisableSound = true,
            ViewFlag = "all"
        }
    };

    public class AdConfig
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("viewFlag")]
        public string? ViewFlag { get; set; } = "all";

        [JsonPropertyName("excludeFlag")]
        public string? ExcludeFlag { get; set; } = "";

        [JsonPropertyName("map")]
        public string Map { get; set; } = "all";

        [JsonPropertyName("interval")]
        public float Interval { get; set; } = 30;

        [JsonPropertyName("disableSound")]
        public bool DisableSound { get; set; } = false;
    }

    public class WelcomeConfig
    {
        [JsonPropertyName("WelcomeMessage")]
        public string WelcomeMessage { get; set; } = string.Empty;

        [JsonPropertyName("viewFlag")]
        public string? ViewFlag { get; set; } = "all";

        [JsonPropertyName("excludeFlag")]
        public string? ExcludeFlag { get; set; } = "";

        [JsonPropertyName("disableSound")]
        public bool DisableSound { get; set; } = false;
    }
}