using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using AutomaticAds.Models;
using System.Text.RegularExpressions;

namespace AutomaticAds.Utils;

public class MessageFormatter
{
    private static readonly Dictionary<string, string> ColorMappings = new()
    {
        { "{GREEN}", ChatColors.Green.ToString() },
        { "{RED}", ChatColors.Red.ToString() },
        { "{YELLOW}", ChatColors.Yellow.ToString() },
        { "{BLUE}", ChatColors.Blue.ToString() },
        { "{PURPLE}", ChatColors.Purple.ToString() },
        { "{ORANGE}", ChatColors.Orange.ToString() },
        { "{WHITE}", ChatColors.White.ToString() },
        { "{NORMAL}", ChatColors.White.ToString() },
        { "{GREY}", ChatColors.Grey.ToString() },
        { "{LIGHT_RED}", ChatColors.LightRed.ToString() },
        { "{LIGHT_BLUE}", ChatColors.LightBlue.ToString() },
        { "{LIGHT_PURPLE}", ChatColors.LightPurple.ToString() },
        { "{LIGHT_YELLOW}", ChatColors.LightYellow.ToString() },
        { "{DARK_RED}", ChatColors.DarkRed.ToString() },
        { "{DARK_BLUE}", ChatColors.DarkBlue.ToString() },
        { "{BLUE_GREY}", ChatColors.BlueGrey.ToString() },
        { "{OLIVE}", ChatColors.Olive.ToString() },
        { "{LIME}", ChatColors.Lime.ToString() },
        { "{GOLD}", ChatColors.Gold.ToString() },
        { "{SILVER}", ChatColors.Silver.ToString() },
        { "{MAGENTA}", ChatColors.Magenta.ToString() }
    };

    public string FormatMessage(string message, string playerName = "", string chatPrefix = "")
    {
        if (ColorMappings.Keys.Any(color => message.StartsWith(color)))
        {
            message = " " + message;
        }

        message = ProcessPrefix(message, chatPrefix);
        message = ApplyColors(message);
        message = ReplaceServerVariables(message);
        message = ReplacePlayerName(message, playerName);
        message = message.Replace("\n", "\u2029");

        return message;
    }

    public string FormatMessageWithPlayerInfo(string message, PlayerInfo playerInfo, string chatPrefix = "")
    {
        message = FormatMessage(message, playerInfo.Name, chatPrefix);
        message = ReplacePlayerVariables(message, playerInfo);
        return message;
    }

    private string ProcessPrefix(string message, string chatPrefix)
    {
        if (!string.IsNullOrWhiteSpace(chatPrefix))
        {
            chatPrefix = ReplaceServerVariables(chatPrefix);
            chatPrefix = ApplyColors(chatPrefix);
            message = message.Replace("{prefix}", chatPrefix);
        }
        else
        {
            message = message.Replace("{prefix}", "");
            if (!ColorMappings.Keys.Any(color => message.StartsWith(color)))
            {
                message = $"{ChatColors.White}{message}";
            }
        }
        return message;
    }

    private string ApplyColors(string message)
    {
        foreach (var colorMapping in ColorMappings)
        {
            message = Regex.Replace(
                message,
                Regex.Escape(colorMapping.Key),
                colorMapping.Value.ToString(),
                RegexOptions.IgnoreCase
            );
        }
        return message;
    }

    private string ReplaceServerVariables(string message)
    {
        var serverVars = GetServerVariables();

        foreach (var variable in serverVars)
        {
            message = message.Replace(variable.Key, variable.Value);
        }

        return message;
    }

    private string ReplacePlayerName(string message, string playerName)
    {
        if (!string.IsNullOrWhiteSpace(playerName))
        {
            message = message.Replace("{playername}", playerName);
        }
        return message;
    }

    private string ReplacePlayerVariables(string message, PlayerInfo playerInfo)
    {
        message = message.Replace("{id64}", playerInfo.SteamId);
        message = message.Replace("{country}", playerInfo.Country ?? Constants.ErrorMessages.Unknown);
        return message;
    }

    private Dictionary<string, string> GetServerVariables()
    {
        try
        {
            string ip = GetServerIp();
            string hostname = GetServerHostname();
            string map = Server.MapName;
            string time = DateTime.Now.ToString("HH:mm");
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            int players = GetPlayerCount();
            int maxPlayers = Server.MaxPlayers;

            return new Dictionary<string, string>
            {
                { "{ip}", ip },
                { "{hostname}", hostname },
                { "{map}", map },
                { "{time}", time },
                { "{date}", date },
                { "{players}", players.ToString() },
                { "{maxplayers}", maxPlayers.ToString() }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error getting server variables: {ex.Message}");
            return GetFallbackServerVariables();
        }
    }

    private string GetServerIp()
    {
        try
        {
            var ipCvar = ConVar.Find("ip");
            var portCvar = ConVar.Find("hostport");
            string ip = ipCvar?.StringValue ?? "Unknown";
            int port = portCvar?.GetPrimitiveValue<int>() ?? 27015;
            return $"{ip}:{port}";
        }
        catch
        {
            return "Unknown:27015";
        }
    }

    private string GetServerHostname()
    {
        try
        {
            return ConVar.Find("hostname")?.StringValue ?? Constants.ErrorMessages.Unknown;
        }
        catch
        {
            return Constants.ErrorMessages.Unknown;
        }
    }

    private int GetPlayerCount()
    {
        try
        {
            return Utilities.GetPlayers().Count(p => !p.IsBot && !p.IsHLTV);
        }
        catch
        {
            return 0;
        }
    }

    private Dictionary<string, string> GetFallbackServerVariables()
    {
        return new Dictionary<string, string>
        {
            { "{ip}", "Unknown:27015" },
            { "{hostname}", Constants.ErrorMessages.Unknown },
            { "{map}", Server.MapName ?? "Unknown" },
            { "{time}", DateTime.Now.ToString("HH:mm") },
            { "{date}", DateTime.Now.ToString("yyyy-MM-dd") },
            { "{players}", "0" },
            { "{maxplayers}", "0" }
        };
    }
}