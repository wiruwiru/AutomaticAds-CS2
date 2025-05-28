using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

namespace AutomaticAds.Utils;

public static class Extensions
{
    public static bool IsValidPlayer(this CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot && !player.IsHLTV &&
               player.Connected == PlayerConnectedState.PlayerConnected;
    }

    public static List<CCSPlayerController> GetValidPlayers(this IEnumerable<CCSPlayerController> players)
    {
        return players.Where(p => p.IsValidPlayer()).ToList();
    }

    public static string GetPlayerIpAddress(this CCSPlayerController player)
    {
        return player.IpAddress?.Split(':')[0] ?? string.Empty;
    }

    public static bool HasPermission(this CCSPlayerController player, string? flag)
    {
        if (string.IsNullOrWhiteSpace(flag) || flag == Constants.AllPlayersFlag)
            return true;

        return AdminManager.PlayerHasPermissions(player, flag);
    }

    public static bool CanViewMessage(this CCSPlayerController player, string? viewFlag, string? excludeFlag)
    {
        bool canView = player.HasPermission(viewFlag);
        bool isExcluded = !string.IsNullOrWhiteSpace(excludeFlag) && player.HasPermission(excludeFlag);

        return canView && !isExcluded;
    }

    public static bool MapMatches(this string currentMap, string configMap)
    {
        if (configMap == Constants.AllMapsKeyword)
            return true;

        if (configMap.EndsWith("*"))
        {
            string mapPrefix = configMap.Replace("*", "");
            return currentMap.StartsWith(mapPrefix);
        }

        return currentMap.Equals(configMap, StringComparison.OrdinalIgnoreCase);
    }
}