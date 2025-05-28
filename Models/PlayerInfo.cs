namespace AutomaticAds.Models;

public class PlayerInfo
{
    public string Name { get; set; } = string.Empty;
    public string SteamId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? Country { get; set; }

    public PlayerInfo() { }

    public PlayerInfo(string name, string steamId, string ipAddress)
    {
        Name = name;
        SteamId = steamId;
        IpAddress = ipAddress;
    }
}