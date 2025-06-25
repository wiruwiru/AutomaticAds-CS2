using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;
using AutomaticAds.Config.Models;

namespace AutomaticAds.Config;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = "[{GREEN}AutomaticAds{WHITE}]{WHITE}";

    [JsonPropertyName("GlobalPlaySound")]
    public string? GlobalPlaySound { get; set; } = "ui/panorama/popup_reveal_01";

    [JsonPropertyName("GlobalInterval")]
    public float GlobalInterval { get; set; } = 30.0f;

    [JsonPropertyName("AdminFlag")]
    public string? AdminFlag { get; set; } = "@css/generic";

    [JsonPropertyName("SendAdsInOrder")]
    public bool SendAdsInOrder { get; set; } = true;

    [JsonPropertyName("UseWelcomeMessage")]
    public bool EnableWelcomeMessage { get; set; } = true;

    [JsonPropertyName("JoinLeaveMessages")]
    public bool EnableJoinLeaveMessages { get; set; } = true;

    [JsonPropertyName("WelcomeDelay")]
    public float WelcomeDelay { get; set; } = 3.0f;

    [JsonPropertyName("CenterHtmlDisplayTime")]
    public float centerHtmlDisplayTime { get; set; } = 5.0f;

    [JsonPropertyName("ScreenDisplayTime")]
    public float ScreenDisplayTime { get; set; } = 5.0f;

    [JsonPropertyName("UseMultiLang")]
    public bool UseMultiLang { get; set; } = true;

    [JsonPropertyName("DefaultLanguage")]
    public string DefaultLanguage { get; set; } = "en";

    [JsonPropertyName("Welcome")]
    public List<WelcomeConfig> Welcome { get; set; } = new()
    {
        new WelcomeConfig
        {
            WelcomeMessage = "{prefix} {BLUE}Welcome to the server {playername}! {RED}Playing on {map} with {players}/{maxplayers} players.",
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
            JoinMessage = "{BLUE}{playername} ({id64}) joined the server from {country} ({country_code})! {RED}Online: {players}/{maxplayers}.",
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
            DisableOrder = true,
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
}