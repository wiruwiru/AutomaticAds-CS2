using System.Text.Json.Serialization;
using System.Text.Json;

namespace AutomaticAds.Config.Models;

public class WelcomeConfig
{
    private JsonElement? _welcomeMessageElement;

    [JsonPropertyName("WelcomeMessage")]
    public JsonElement WelcomeMessageElement
    {
        get => _welcomeMessageElement ?? new JsonElement();
        set => _welcomeMessageElement = value;
    }

    [JsonIgnore]
    public string WelcomeMessage
    {
        get => GetWelcomeMessage("en");
        set => _welcomeMessageElement = JsonSerializer.SerializeToElement(value);
    }

    [JsonPropertyName("viewFlag")]
    public string? ViewFlag { get; set; } = "all";

    [JsonPropertyName("excludeFlag")]
    public string? ExcludeFlag { get; set; } = "";

    [JsonPropertyName("disableSound")]
    public bool DisableSound { get; set; } = false;

    public string GetWelcomeMessage(string languageCode = "en")
    {
        if (!_welcomeMessageElement.HasValue)
        {
            return string.Empty;
        }

        try
        {
            var element = _welcomeMessageElement.Value;
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(languageCode, out var langMessage) && langMessage.ValueKind == JsonValueKind.String)
                {
                    return langMessage.GetString() ?? string.Empty;
                }

                if (element.TryGetProperty("en", out var enMessage) && enMessage.ValueKind == JsonValueKind.String)
                {
                    return enMessage.GetString() ?? string.Empty;
                }

                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        return prop.Value.GetString() ?? string.Empty;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error parsing Welcome message: {ex.Message}");
        }

        return string.Empty;
    }

    public bool IsMultiLanguage()
    {
        return _welcomeMessageElement.HasValue && _welcomeMessageElement.Value.ValueKind == JsonValueKind.Object;
    }

    public bool HasValidMessage()
    {
        return !string.IsNullOrWhiteSpace(GetWelcomeMessage());
    }
}