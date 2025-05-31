using System.Text.Json.Serialization;
using System.Text.Json;

namespace AutomaticAds.Config.Models;

public class AdConfig
{
    private JsonElement? _messageElement;
    private float? _interval;

    [JsonPropertyName("message")]
    public JsonElement MessageElement
    {
        get => _messageElement ?? new JsonElement();
        set => _messageElement = value;
    }

    [JsonIgnore]
    public string Message
    {
        get => GetMessage("en");
        set
        {
            _messageElement = JsonSerializer.SerializeToElement(value);
        }
    }

    [JsonPropertyName("viewFlag")]
    public string? ViewFlag { get; set; } = "all";

    [JsonPropertyName("excludeFlag")]
    public string? ExcludeFlag { get; set; } = "";

    [JsonPropertyName("map")]
    public string Map { get; set; } = "all";

    [JsonPropertyName("interval")]
    public float? IntervalRaw
    {
        get => _interval;
        set => _interval = value;
    }

    [JsonIgnore]
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

    [JsonPropertyName("disableOrder")]
    public bool DisableOrder { get; set; } = false;

    [JsonPropertyName("playSoundName")]
    public string? PlaySoundName { get; set; } = null;

    [JsonPropertyName("displayType")]
    public DisplayType DisplayType { get; set; } = DisplayType.Chat;

    public bool HasCustomInterval => _interval.HasValue;

    public float GetEffectiveInterval(float globalInterval)
    {
        return _interval ?? globalInterval;
    }

    public string GetMessage(string languageCode = "en")
    {
        if (!_messageElement.HasValue)
        {
            return string.Empty;
        }

        try
        {
            var element = _messageElement.Value;
            if (element.ValueKind == JsonValueKind.Object)
            {

                if (element.TryGetProperty(languageCode, out var langMessage) && langMessage.ValueKind == JsonValueKind.String)
                {
                    var result = langMessage.GetString() ?? string.Empty;
                    return result;
                }

                if (element.TryGetProperty("en", out var enMessage) && enMessage.ValueKind == JsonValueKind.String)
                {
                    var result = enMessage.GetString() ?? string.Empty;
                    return result;
                }

                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var result = prop.Value.GetString() ?? string.Empty;
                        return result;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                var result = element.GetString() ?? string.Empty;
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error parsing message: {ex.Message}");
        }

        return string.Empty;
    }

    public bool IsMultiLanguage()
    {
        if (_messageElement.HasValue && _messageElement.Value.ValueKind == JsonValueKind.Object)
        {
            return _messageElement.Value.EnumerateObject().Any();
        }
        return false;
    }

    public bool HasValidMessage()
    {
        return !string.IsNullOrWhiteSpace(GetMessage());
    }
}