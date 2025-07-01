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
        string effectiveViewFlag = viewFlag ?? Constants.AllPlayersFlag;
        string effectiveExcludeFlag = excludeFlag ?? string.Empty;

        bool canView = player.HasPermission(effectiveViewFlag);
        bool isExcluded = !string.IsNullOrWhiteSpace(effectiveExcludeFlag) && player.HasPermission(effectiveExcludeFlag);

        return canView && !isExcluded;
    }

    public static bool MapMatches(this string currentMap, string? configMap)
    {
        string effectiveMap = configMap ?? Constants.AllMapsKeyword;

        if (effectiveMap == Constants.AllMapsKeyword)
            return true;

        if (effectiveMap.EndsWith("*"))
        {
            string mapPrefix = effectiveMap.Replace("*", "");
            return currentMap.StartsWith(mapPrefix);
        }

        return currentMap.Equals(effectiveMap, StringComparison.OrdinalIgnoreCase);
    }
}