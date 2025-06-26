using System.Text.Json.Serialization;
using System.Text.Json;

namespace AutomaticAds.Config.Models;

public class AdConfig
{
    private JsonElement? _messageElement;
    private float? _interval;
    private float? _positionX;
    private float? _positionY;

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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ViewFlag { get; set; } = null;

    [JsonPropertyName("excludeFlag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExcludeFlag { get; set; } = null;

    [JsonPropertyName("map")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Map { get; set; } = null;

    [JsonPropertyName("interval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? IntervalRaw
    {
        get => _interval;
        set => _interval = value;
    }

    [JsonIgnore]
    public float Interval { get; set; } = 30;

    [JsonPropertyName("positionX")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? PositionXRaw
    {
        get => _positionX;
        set => _positionX = value;
    }

    [JsonIgnore]
    public float PositionX { get; set; } = -1.5f;

    [JsonPropertyName("positionY")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? PositionYRaw
    {
        get => _positionY;
        set => _positionY = value;
    }

    [JsonIgnore]
    public float PositionY { get; set; } = 1f;

    [JsonPropertyName("disableSound")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool DisableSound { get; set; } = false;

    [JsonPropertyName("onlyInWarmup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool OnlyInWarmup { get; set; } = false;

    [JsonPropertyName("onlySpec")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool onlySpec { get; set; } = false;

    [JsonPropertyName("onDead")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool onDead { get; set; } = false;

    [JsonPropertyName("triggerAd")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? TriggerAd { get; set; } = null;

    [JsonPropertyName("disableInterval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool DisableInterval { get; set; } = false;

    [JsonPropertyName("disableOrder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool DisableOrder { get; set; } = false;

    [JsonPropertyName("playSoundName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PlaySoundName { get; set; } = null;

    [JsonPropertyName("displayType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DisplayType DisplayType { get; set; } = DisplayType.Chat;

    [JsonIgnore]
    public bool HasCustomInterval => _interval.HasValue;

    [JsonIgnore]
    public bool HasCustomPositionX => _positionX.HasValue;

    [JsonIgnore]
    public bool HasCustomPositionY => _positionY.HasValue;

    public float GetEffectiveInterval(float globalInterval)
    {
        return _interval ?? globalInterval;
    }

    public float GetEffectivePositionX(float defaultPositionX)
    {
        return _positionX ?? defaultPositionX;
    }

    public float GetEffectivePositionY(float defaultPositionY)
    {
        return _positionY ?? defaultPositionY;
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