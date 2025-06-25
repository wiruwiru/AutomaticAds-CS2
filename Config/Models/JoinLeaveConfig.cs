using System.Text.Json.Serialization;
using System.Text.Json;

namespace AutomaticAds.Config.Models;

public class JoinLeaveConfig
{
    private JsonElement? _joinMessageElement;
    private JsonElement? _leaveMessageElement;

    [JsonPropertyName("JoinMessage")]
    public JsonElement JoinMessageElement
    {
        get => _joinMessageElement ?? new JsonElement();
        set => _joinMessageElement = value;
    }

    [JsonPropertyName("LeaveMessage")]
    public JsonElement LeaveMessageElement
    {
        get => _leaveMessageElement ?? new JsonElement();
        set => _leaveMessageElement = value;
    }

    [JsonIgnore]
    public string JoinMessage
    {
        get => GetJoinMessage("en");
        set => _joinMessageElement = JsonSerializer.SerializeToElement(value);
    }

    [JsonIgnore]
    public string LeaveMessage
    {
        get => GetLeaveMessage("en");
        set => _leaveMessageElement = JsonSerializer.SerializeToElement(value);
    }

    public string GetJoinMessage(string languageCode = "en")
    {
        return GetMessageFromElement(_joinMessageElement, languageCode);
    }

    public string GetLeaveMessage(string languageCode = "en")
    {
        return GetMessageFromElement(_leaveMessageElement, languageCode);
    }

    private string GetMessageFromElement(JsonElement? messageElement, string languageCode)
    {
        if (!messageElement.HasValue)
        {
            return string.Empty;
        }

        try
        {
            var element = messageElement.Value;
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
            Console.WriteLine($"[AutomaticAds] Error parsing JoinLeave message: {ex.Message}");
        }

        return string.Empty;
    }

    public bool IsJoinMessageMultiLanguage()
    {
        return _joinMessageElement.HasValue && _joinMessageElement.Value.ValueKind == JsonValueKind.Object;
    }

    public bool IsLeaveMessageMultiLanguage()
    {
        return _leaveMessageElement.HasValue && _leaveMessageElement.Value.ValueKind == JsonValueKind.Object;
    }

    public bool HasValidJoinMessage()
    {
        return !string.IsNullOrWhiteSpace(GetJoinMessage());
    }

    public bool HasValidLeaveMessage()
    {
        return !string.IsNullOrWhiteSpace(GetLeaveMessage());
    }
}