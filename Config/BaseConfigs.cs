using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AutomaticAds.Config;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = "[{GREEN}AutomaticAds{WHITE}]{WHITE}";

    [JsonPropertyName("GlobalPlaySound")]
    public string? GlobalPlaySound { get; set; } = "ui/panorama/popup_reveal_01";

    [JsonPropertyName("sendAdsInOrder")]
    public bool SendAdsInOrder { get; set; } = true;

    [JsonPropertyName("UseWelcomeMessage")]
    public bool EnableWelcomeMessage { get; set; } = true;

    [JsonPropertyName("JoinLeaveMessages")]
    public bool EnableJoinLeaveMessages { get; set; } = true;

    [JsonPropertyName("WelcomeDelay")]
    public float WelcomeDelay { get; set; } = 5.0f;

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

    [JsonPropertyName("JoinLeave")]
    public List<JoinLeaveConfig> JoinLeave { get; set; } = new()
    {
        new JoinLeaveConfig
        {
            JoinMessage = "{BLUE}{playername} ({id64}) joined the server from {country}! {RED}Online: {players}/{maxplayers}.",
            LeaveMessage = "{BLUE}{playername} ({id64}) left the server!",
        }
    };

    [JsonPropertyName("Ads")]
    public List<AdConfig> Ads { get; set; } = new()
    {
        new AdConfig
        {
            Message = "{prefix} {RED}AutomaticAds is the best plugin!",
            Map = "all",
            Interval = 600,
            DisableSound = false,
            ViewFlag = "all"
        },
        new AdConfig
        {
            Message = "{prefix} {BLUE}Welcome to {hostname}! {RED}The time is {time} of {date}, playing in {map} with {players}/{maxplayers}. Connect {ip}",
            Map = "all",
            Interval = 800,
            DisableSound = false,
            ViewFlag = "all"
        },
        new AdConfig
        {
            Message = "{prefix} {BLUE}Thank you for supporting the server! {GOLD}Your contribution is greatly appreciated.",
            Map = "all",
            Interval = 1000,
            DisableSound = true,
            ViewFlag = "@css/vip"
        },
        new AdConfig
        {
            Message = "{prefix} {GOLD}Congratulations {playername}, you are playing on Mirage.",
            ExcludeFlag = "@css/vip",
            Map = "de_mirage",
            Interval = 1400,
            DisableSound = true,
            ViewFlag = "all",
            TriggerAd = ["map", "currentmap"],
            PlaySoundName = "sound/ui/beep22.wav"
        },
        new AdConfig
        {
            Message = "<font class='fontSize-m' color='orange'>This server uses</font><br><font class='fontSize-l' style='color:red;'>AutomaticAds</font></font>",
            ViewFlag = "@css/generic",
            DisplayType = DisplayType.CenterHtml,
            onDead = true
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

        [JsonPropertyName("onlyInWarmup")]
        public bool OnlyInWarmup { get; set; } = false;

        [JsonPropertyName("onlySpec")]
        public bool onlySpec { get; set; } = false;

        [JsonPropertyName("onDead")]
        public bool onDead { get; set; } = false;

        [JsonPropertyName("triggerAd")]
        public List<string>? TriggerAd { get; set; } = new();

        [JsonPropertyName("Disableinterval")]
        public bool DisableInterval { get; set; } = false;

        [JsonPropertyName("playSoundName")]
        public string? PlaySoundName { get; set; } = null;

        [JsonPropertyName("displayType")]
        public DisplayType DisplayType { get; set; } = DisplayType.Chat;
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

    public class JoinLeaveConfig
    {
        [JsonPropertyName("JoinMessage")]
        public string JoinMessage { get; set; } = string.Empty;

        [JsonPropertyName("LeaveMessage")]
        public string LeaveMessage { get; set; } = string.Empty;
    }
}