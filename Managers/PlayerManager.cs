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

    public bool ShouldQueryCountryInfo()
    {
        return _plugin?.Config?.EnableJoinLeaveMessages == true ||
               _plugin?.Config?.UseMultiLang == true;
    }

    public async Task<PlayerInfo> GetOrCreatePlayerInfoAsync(CCSPlayerController player, Services.IIPQueryService? ipQueryService = null)
    {
        try
        {
            ulong steamId = player.SteamID;

            var cachedInfo = GetCachedPlayerInfo(steamId);
            if (cachedInfo != null && IsValidCachedInfo(cachedInfo))
                return cachedInfo;

            var playerInfo = CreatePlayerInfo(player);
            await EnrichWithCountryInfoIfNeeded(playerInfo, ipQueryService);
            UpdateCache(steamId, playerInfo);

            return playerInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error getting/creating player info: {ex.Message}");
            return CreatePlayerInfo(player);
        }
    }

    public PlayerInfo GetBasicPlayerInfo(CCSPlayerController player)
    {
        try
        {
            ulong steamId = player.SteamID;

            if (_playerInfoCache.TryGetValue(steamId, out var cachedInfo))
                return cachedInfo;

            var basicInfo = CreateBasicPlayerInfo(player);
            AddToCache(steamId, basicInfo);

            return basicInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error getting basic player info: {ex.Message}");
            return CreatePlayerInfo(player);
        }
    }

    private PlayerInfo? GetCachedPlayerInfo(ulong steamId)
    {
        return _playerInfoCache.TryGetValue(steamId, out var cachedInfo) ? cachedInfo : null;
    }

    private bool IsValidCachedInfo(PlayerInfo? cachedInfo)
    {
        if (cachedInfo == null) return false;
        if (!ShouldQueryCountryInfo()) return true;

        return !string.IsNullOrEmpty(cachedInfo.CountryCode) &&
               cachedInfo.CountryCode != Utils.Constants.ErrorMessages.Unknown;
    }

    private async Task EnrichWithCountryInfoIfNeeded(PlayerInfo playerInfo, Services.IIPQueryService? ipQueryService)
    {
        var shouldQuery = ShouldQueryCountryInfo() &&
                         ipQueryService != null &&
                         !string.IsNullOrEmpty(playerInfo.IpAddress);

        if (!shouldQuery)
        {
            SetDefaultCountryInfo(playerInfo);
            return;
        }

        if (ipQueryService != null)
        {
            await SetCountryInfoFromApi(playerInfo, ipQueryService);
        }
        else
        {
            SetDefaultCountryInfo(playerInfo);
        }
    }

    private async Task SetCountryInfoFromApi(PlayerInfo playerInfo, Services.IIPQueryService ipQueryService)
    {
        var countryCode = await ipQueryService.GetCountryCodeAsync(playerInfo.IpAddress);
        var isValidCountryCode = countryCode != Utils.Constants.ErrorMessages.CountryCodeError;

        playerInfo.CountryCode = isValidCountryCode ? countryCode : Utils.Constants.ErrorMessages.Unknown;
        playerInfo.CountryName = isValidCountryCode ? CountryMapping.GetCountryName(countryCode) : Utils.Constants.ErrorMessages.Unknown;
    }

    private void SetDefaultCountryInfo(PlayerInfo playerInfo)
    {
        playerInfo.CountryCode = _plugin?.Config?.DefaultLanguage ?? "en";
        playerInfo.CountryName = string.Empty;
    }

    private PlayerInfo CreateBasicPlayerInfo(CCSPlayerController player)
    {
        var basicInfo = CreatePlayerInfo(player);
        basicInfo.CountryCode = _plugin?.Config?.DefaultLanguage ?? "en";
        return basicInfo;
    }

    private void UpdateCache(ulong steamId, PlayerInfo playerInfo)
    {
        var now = DateTime.Now;
        _playerInfoCache.AddOrUpdate(steamId, playerInfo, (key, oldValue) => playerInfo);
        _cacheTimestamps.AddOrUpdate(steamId, now, (key, oldValue) => now);
    }

    private void AddToCache(ulong steamId, PlayerInfo playerInfo)
    {
        _playerInfoCache.TryAdd(steamId, playerInfo);
        _cacheTimestamps.TryAdd(steamId, DateTime.Now);
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

    public void PlaySoundToPlayer(CCSPlayerController player, string soundName)
    {
        if (player.IsValidPlayer() && !string.IsNullOrWhiteSpace(soundName))
        {
            Server.NextFrame(() =>
            {
                try
                {
                    player.ExecuteClientCommand($"play {soundName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutomaticAds] Error playing sound to player: {ex.Message}");
                }
            });
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

        Server.NextFrame(() =>
        {
            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine($"[AutomaticAds] Error sending message to player: {ex.Message}");
            }
        });
    }
}