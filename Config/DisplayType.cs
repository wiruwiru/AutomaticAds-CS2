using System.Text.Json.Serialization;

namespace AutomaticAds.Config;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisplayType
{
    Chat,
    Center,
    CenterHtml
}