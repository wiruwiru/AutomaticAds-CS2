using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Admin;
using AutomaticAds.Models;
using AutomaticAds.Config;
using AutomaticAds.Config.Models;
using System.Text.RegularExpressions;

namespace AutomaticAds.Utils;

public class MessageFormatter
{
    private readonly BaseConfigs? _config;
    private string _currentMap = string.Empty;
    private Dictionary<string, string> _cachedServerVariables = new();
    private DateTime _lastServerVarUpdate = DateTime.MinValue;

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

    public void SetCurrentMap(string mapName)
    {
        _currentMap = mapName;
    }

    public MessageFormatter(BaseConfigs? config = null)
    {
        _config = config;
    }

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

    public string FormatAdMessage(AdConfig ad, PlayerInfo playerInfo, string chatPrefix = "")
    {
        try
        {
            string languageCode = GetPlayerLanguage(playerInfo);
            string message = ad.GetMessage(languageCode);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ad.GetMessage("en");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            return FormatMessageWithPlayerInfo(message, playerInfo, chatPrefix);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error formatting ad message: {ex.Message}");
            return string.Empty;
        }
    }

    public string FormatAdMessage(AdConfig ad, string playerName = "", string countryCode = "", string chatPrefix = "")
    {
        try
        {
            string languageCode = GetLanguageFromCountryCode(countryCode);
            string message = ad.GetMessage(languageCode);
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ad.GetMessage("en");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            return FormatMessage(message, playerName, chatPrefix);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error formatting ad message: {ex.Message}");
            return string.Empty;
        }
    }

    private string GetPlayerLanguage(PlayerInfo playerInfo)
    {
        try
        {
            if (string.IsNullOrEmpty(playerInfo.CountryCode))
            {
                string defaultLang = _config?.DefaultLanguage ?? "en";
                return defaultLang;
            }

            if (_config?.UseMultiLang == true)
            {
                string language = GetLanguageFromCountryCode(playerInfo.CountryCode);
                return language;
            }
            else
            {
                string defaultLang = _config?.DefaultLanguage ?? "en";
                return defaultLang;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error getting player language: {ex.Message}");
            return "en";
        }
    }

    private string GetLanguageFromCountryCode(string countryCode)
    {
        try
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                string defaultLang = _config?.DefaultLanguage ?? "en";
                return defaultLang;
            }

            string mappedLanguage = CountryMapping.GetCountryLanguage(countryCode);
            if (string.IsNullOrEmpty(mappedLanguage))
            {
                string defaultLang = _config?.DefaultLanguage ?? "en";
                return defaultLang;
            }

            return mappedLanguage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error mapping country code to language: {ex.Message}");
            return _config?.DefaultLanguage ?? "en";
        }
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
        message = message.Replace("{country}", playerInfo.CountryName ?? Constants.ErrorMessages.Unknown);
        message = message.Replace("{country_code}", playerInfo.CountryCode ?? Constants.ErrorMessages.Unknown);
        return message;
    }

    public Dictionary<string, string> GetServerVariables()
    {
        bool needsUpdate = (DateTime.Now - _lastServerVarUpdate).TotalSeconds > 5 || _cachedServerVariables.Count == 0;

        if (needsUpdate)
        {
            try
            {
                var newVariables = GetServerVariablesInternal();
                if (newVariables.Count > 0)
                {
                    _cachedServerVariables = newVariables;
                    _lastServerVarUpdate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AutomaticAds] Error updating server variables: {ex.Message}");
                if (_cachedServerVariables.Count == 0)
                {
                    _cachedServerVariables = GetFallbackServerVariables();
                }
            }
        }

        return _cachedServerVariables.Count > 0 ? _cachedServerVariables : GetFallbackServerVariables();
    }

    private Dictionary<string, string> GetServerVariablesInternal()
    {
        try
        {
            string ip = "Unknown:27015";
            string port = "27015";
            string hostname = "Unknown";
            string map = _currentMap ?? "Unknown";
            string time = DateTime.Now.ToString("HH:mm");
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            int players = 0;
            int maxPlayers = 0;

            var ipCvar = ConVar.Find("ip");
            var portCvar = ConVar.Find("hostport");
            var hostnameCvar = ConVar.Find("hostname");

            if (ipCvar != null && portCvar != null)
            {
                string serverIp = ipCvar.StringValue ?? "Unknown";
                int serverPort = portCvar.GetPrimitiveValue<int>();
                ip = $"{serverIp}:{serverPort}";
                port = serverPort.ToString();
            }

            if (hostnameCvar != null)
            {
                hostname = hostnameCvar.StringValue ?? "Unknown";
            }

            players = Utilities.GetPlayers().Count(p => p.IsValid && !p.IsBot && !p.IsHLTV);
            maxPlayers = Server.MaxPlayers;

            var adminInfo = GetAdministratorInfo();
            int adminCount = adminInfo.Count;
            string adminNames = adminInfo.Count > 0 ? string.Join(", ", adminInfo) : "None";

            return new Dictionary<string, string>
            {
                { "{ip}", ip },
                { "{port}", port },
                { "{hostname}", hostname },
                { "{map}", map },
                { "{time}", time },
                { "{date}", date },
                { "{players}", players.ToString() },
                { "{maxplayers}", maxPlayers.ToString() },
                { "{admincount}", adminCount.ToString() },
                { "{adminnames}", adminNames }
            };
        }
        catch
        {
            return GetFallbackServerVariables();
        }
    }

    private List<string> GetAdministratorInfo()
    {
        try
        {
            var administrators = new List<string>();
            string adminFlag = _config?.AdminFlag ?? "@css/generic";

            var validPlayers = Utilities.GetPlayers()
                .Where(p => p.IsValidPlayer())
                .ToList();

            foreach (var player in validPlayers)
            {
                try
                {
                    if (AdminManager.PlayerHasPermissions(player, adminFlag))
                    {
                        administrators.Add(player.PlayerName ?? "Unknown");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutomaticAds] Error checking admin permissions for player {player.PlayerName}: {ex.Message}");
                    continue;
                }
            }

            return administrators;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error getting administrator info: {ex.Message}");
            return new List<string>();
        }
    }

    private Dictionary<string, string> GetFallbackServerVariables()
    {
        return new Dictionary<string, string>
        {
            { "{ip}", "Unknown:27015" },
            { "{port}", "27015" },
            { "{hostname}", Constants.ErrorMessages.Unknown },
            { "{map}", Server.MapName ?? "Unknown" },
            { "{time}", DateTime.Now.ToString("HH:mm") },
            { "{date}", DateTime.Now.ToString("yyyy-MM-dd") },
            { "{players}", "0" },
            { "{maxplayers}", "0" },
            { "{admincount}", "0" },
            { "{adminnames}", "None" }
        };
    }
}