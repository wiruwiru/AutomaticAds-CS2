using System.Text.Json.Serialization;

namespace AutomaticAds.Config.Models;

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