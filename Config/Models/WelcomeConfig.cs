using System.Text.Json.Serialization;

namespace AutomaticAds.Config.Models;

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