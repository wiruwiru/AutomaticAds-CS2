using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using AutomaticAds.Models;
using AutomaticAds.Config;
using AutomaticAds.Utils;
using System.Collections.Concurrent;

namespace AutomaticAds.Managers;

public class PlayerManager
{
    private readonly AutomaticAdsBase? _plugin;
    private readonly ConcurrentDictionary<ulong, PlayerInfo> _playerInfoCache = new();
    private readonly ConcurrentDictionary<ulong, DateTime> _cacheTimestamps = new();

    public PlayerManager(AutomaticAdsBase? plugin = null)
    {
        _plugin = plugin;
    }

    public List<CCSPlayerController> GetValidPlayers()
    {
        return Utilities.GetPlayers().GetValidPlayers();
    }

    public PlayerInfo GetOrCreatePlayerInfo(CCSPlayerController player)
    {
        try
        {
            ulong steamId = player.SteamID;
            if (_playerInfoCache.TryGetValue(steamId, out var cachedInfo))
            {
                return cachedInfo;
            }

            var basicInfo = CreatePlayerInfo(player);
            _playerInfoCache.TryAdd(steamId, basicInfo);
            _cacheTimestamps.TryAdd(steamId, DateTime.Now);

            return basicInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error getting player info from cache: {ex.Message}");
            return CreatePlayerInfo(player);
        }
    }

    public async Task<PlayerInfo> UpdatePlayerInfoWithCountryAsync(CCSPlayerController player, Services.IIPQueryService ipQueryService)
    {
        try
        {
            ulong steamId = player.SteamID;
            var playerInfo = await CreatePlayerInfoWithCountryAsync(player, ipQueryService);

            _playerInfoCache.AddOrUpdate(steamId, playerInfo, (key, oldValue) => playerInfo);
            _cacheTimestamps.AddOrUpdate(steamId, DateTime.Now, (key, oldValue) => DateTime.Now);

            return playerInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error updating player info with country: {ex.Message}");
            return GetOrCreatePlayerInfo(player);
        }
    }

    public bool NeedsCountryUpdate(ulong steamId)
    {
        if (!_playerInfoCache.TryGetValue(steamId, out var playerInfo))
            return true;

        return string.IsNullOrEmpty(playerInfo.CountryCode) ||
               playerInfo.CountryCode == Utils.Constants.ErrorMessages.Unknown;
    }

    public void ClearPlayerCache(CCSPlayerController player)
    {
        try
        {
            ulong steamId = player.SteamID;
            _playerInfoCache.TryRemove(steamId, out _);
            _cacheTimestamps.TryRemove(steamId, out _);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error clearing player cache: {ex.Message}");
        }
    }

    public void ClearAllCache()
    {
        _playerInfoCache.Clear();
        _cacheTimestamps.Clear();
    }

    public void CleanupOldCacheEntries(TimeSpan maxAge)
    {
        var cutoffTime = DateTime.Now - maxAge;
        var expiredKeys = _cacheTimestamps
            .Where(kvp => kvp.Value < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _playerInfoCache.TryRemove(key, out _);
            _cacheTimestamps.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            Console.WriteLine($"[AutomaticAds] Cleaned up {expiredKeys.Count} expired cache entries");
        }
    }

    public PlayerInfo CreatePlayerInfo(CCSPlayerController player)
    {
        return new PlayerInfo
        {
            Name = player.PlayerName ?? string.Empty,
            SteamId = player.SteamID.ToString(),
            IpAddress = player.GetPlayerIpAddress(),
            CountryCode = _plugin?.Config?.DefaultLanguage ?? "en"
        };
    }

    public async Task<PlayerInfo> CreatePlayerInfoWithCountryAsync(CCSPlayerController player, Services.IIPQueryService ipQueryService)
    {
        var playerInfo = CreatePlayerInfo(player);

        if (!string.IsNullOrEmpty(playerInfo.IpAddress))
        {
            string countryCode = await ipQueryService.GetCountryCodeAsync(playerInfo.IpAddress);

            if (countryCode != Utils.Constants.ErrorMessages.CountryCodeError)
            {
                playerInfo.CountryCode = countryCode;
                playerInfo.CountryName = CountryMapping.GetCountryName(countryCode);
            }
            else
            {
                playerInfo.CountryCode = Utils.Constants.ErrorMessages.Unknown;
                playerInfo.CountryName = Utils.Constants.ErrorMessages.Unknown;
            }
        }
        else
        {
            playerInfo.CountryCode = Utils.Constants.ErrorMessages.Unknown;
            playerInfo.CountryName = Utils.Constants.ErrorMessages.Unknown;
        }

        return playerInfo;
    }

    public void PlaySoundToPlayer(CCSPlayerController player, string soundName)
    {
        if (player.IsValidPlayer() && !string.IsNullOrWhiteSpace(soundName))
        {
            player.ExecuteClientCommand($"play {soundName}");
        }
    }

    public void SendMessageToPlayer(CCSPlayerController player, string message)
    {
        if (player.IsValidPlayer() && !string.IsNullOrWhiteSpace(message))
        {
            player.PrintToChat(message);
        }
    }

    public void SendMessageToPlayer(CCSPlayerController player, string message, DisplayType displayType)
    {
        if (!player.IsValidPlayer() || string.IsNullOrWhiteSpace(message))
            return;

        switch (displayType)
        {
            case DisplayType.Chat:
                player.PrintToChat(message);
                break;
            case DisplayType.Center:
                player.PrintToCenterAlert(message);
                break;
            case DisplayType.CenterHtml:
                _plugin?.StartCenterHtmlMessage(player, message);
                break;
            default:
                player.PrintToChat(message);
                break;
        }
    }
}