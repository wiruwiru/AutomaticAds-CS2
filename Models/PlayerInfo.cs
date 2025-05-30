namespace AutomaticAds.Models;

public class PlayerInfo
{
    public string Name { get; set; } = string.Empty;
    public string SteamId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;

    public PlayerInfo() { }

    public PlayerInfo(string name, string steamId, string ipAddress)
    {
        Name = name;
        SteamId = steamId;
        IpAddress = ipAddress;
    }

    public PlayerInfo(string name, string steamId, string ipAddress, string countryCode, string countryName)
    {
        Name = name;
        SteamId = steamId;
        IpAddress = ipAddress;
        CountryCode = countryCode;
        CountryName = countryName;
    }
}