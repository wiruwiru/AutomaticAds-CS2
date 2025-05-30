using System.Text.Json.Serialization;

namespace AutomaticAds.Config.Models;

public class JoinLeaveConfig
{
    [JsonPropertyName("JoinMessage")]
    public string JoinMessage { get; set; } = string.Empty;

    [JsonPropertyName("LeaveMessage")]
    public string LeaveMessage { get; set; } = string.Empty;
}