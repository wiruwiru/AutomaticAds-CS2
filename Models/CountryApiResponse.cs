using Newtonsoft.Json;

namespace AutomaticAds.Models;

public class CountryApiResponse
{
    [JsonProperty("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;
}