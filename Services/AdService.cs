using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;

using AutomaticAds.Config;
using AutomaticAds.Config.Models;
using AutomaticAds.Managers;
using AutomaticAds.Utils;

namespace AutomaticAds.Services;

public enum AdType
{
    Interval,
    Spectator,
    OnDead
}

public class AdScheduler
{
    private readonly TimerManager _timerManager;
    private readonly Dictionary<AdType, int> _currentIndexes = new();
    private readonly Dictionary<AdType, List<AdConfig>> _orderedAds = new();

    public AdScheduler(TimerManager timerManager)
    {
        _timerManager = timerManager;
        InitializeIndexes();
    }

    private void InitializeIndexes()
    {
        _currentIndexes[AdType.Interval] = 0;
        _currentIndexes[AdType.Spectator] = 0;
        _currentIndexes[AdType.OnDead] = 0;
    }

    public void SetOrderedAds(AdType adType, List<AdConfig> ads)
    {
        _orderedAds[adType] = ads;
        _currentIndexes[adType] = 0;
    }

    public void ScheduleOrderedAds(AdType adType, Func<AdConfig, bool> canSendAd, Action<AdConfig> sendAd)
    {
        if (!_orderedAds.ContainsKey(adType) || !_orderedAds[adType].Any())
            return;

        ScheduleNextOrderedAd(adType, canSendAd, sendAd);
    }

    private void ScheduleNextOrderedAd(AdType adType, Func<AdConfig, bool> canSendAd, Action<AdConfig> sendAd)
    {
        var ads = _orderedAds[adType];
        var currentAd = ads[_currentIndexes[adType]];

        var timer = _timerManager.AddTimer(currentAd.Interval, () =>
        {
            if (canSendAd(currentAd))
            {
                sendAd(currentAd);
            }

            _currentIndexes[adType] = (_currentIndexes[adType] + 1) % ads.Count;
            ScheduleNextOrderedAd(adType, canSendAd, sendAd);
        });

        SetTimerByType(adType, timer);
    }

    private void SetTimerByType(AdType adType, CounterStrikeSharp.API.Modules.Timers.Timer timer)
    {
        switch (adType)
        {
            case AdType.Interval:
                _timerManager.SetAdTimer(timer);
                break;
            case AdType.Spectator:
                _timerManager.SetSpecAdTimer(timer);
                break;
            case AdType.OnDead:
                _timerManager.SetOnDeadAdTimer(timer);
                break;
        }
    }

    public void ScheduleIndividualAds(List<AdConfig> ads, Func<AdConfig, bool> canSendAd, Action<AdConfig> sendAd)
    {
        foreach (var ad in ads)
        {
            _timerManager.AddTimer(Math.Max(1.0f, ad.Interval / 10.0f), () =>
            {
                if (canSendAd(ad))
                {
                    sendAd(ad);
                }
            }, TimerFlags.REPEAT);
        }
    }
}

public class AdService
{
    private readonly BaseConfigs _config;
    private readonly MessageFormatter _messageFormatter;
    private readonly PlayerManager _playerManager;
    private readonly IIPQueryService _ipQueryService;
    private readonly AdScheduler _adScheduler;
    private readonly Dictionary<AdConfig, DateTime> _lastAdTimes = new();
    private string _currentMap = string.Empty;
    private CCSGameRules? _gameRules;

    public AdService(BaseConfigs config, MessageFormatter messageFormatter, TimerManager timerManager,
                    PlayerManager playerManager, IIPQueryService ipQueryService)
    {
        _config = config;
        _messageFormatter = messageFormatter;
        _playerManager = playerManager;
        _ipQueryService = ipQueryService;
        _adScheduler = new AdScheduler(timerManager);

        InitializeAdTimes();
        InitializeIntervals();
    }

    public void SetCurrentMap(string mapName) => _currentMap = mapName;

    public void SetGameRules(CCSGameRules? gameRules) => _gameRules = gameRules;

    public void StartAdvertising()
    {
        if (_config.SendAdsInOrder)
        {
            StartOrderedAdvertising();
        }
        else
        {
            StartIntervalBasedAdvertising();
        }
    }

    private void StartOrderedAdvertising()
    {
        var adTypes = new[] { AdType.Interval, AdType.Spectator, AdType.OnDead };

        foreach (var adType in adTypes)
        {
            var (orderedAds, unorderedAds) = GetOrderedAndUnorderedAds(adType);

            if (orderedAds.Any())
            {
                _adScheduler.SetOrderedAds(adType, orderedAds);
                _adScheduler.ScheduleOrderedAds(adType, CanSendAd,
                    ad => SendAdToPlayers(ad, GetTargetPlayersForAdType(adType, ad)));
            }

            if (unorderedAds.Any())
            {
                _adScheduler.ScheduleIndividualAds(unorderedAds, CanSendAd,
                    ad => SendAdToPlayers(ad, GetTargetPlayersForAdType(adType, ad)));
            }
        }
    }

    private void StartIntervalBasedAdvertising()
    {
        var adTypes = new[] { AdType.Interval, AdType.Spectator, AdType.OnDead };

        foreach (var adType in adTypes)
        {
            var ads = GetAdsByType(adType);
            if (ads.Any())
            {
                _adScheduler.ScheduleIndividualAds(ads, CanSendAd,
                    ad => SendAdToPlayers(ad, GetTargetPlayersForAdType(adType, ad)));
            }
        }
    }

    private (List<AdConfig> ordered, List<AdConfig> unordered) GetOrderedAndUnorderedAds(AdType adType)
    {
        var allAds = GetAdsByType(adType);
        var ordered = allAds.Where(ad => !ad.DisableOrder).ToList();
        var unordered = allAds.Where(ad => ad.DisableOrder).ToList();
        return (ordered, unordered);
    }

    private List<AdConfig> GetAdsByType(AdType adType)
    {
        return adType switch
        {
            AdType.Interval => _config.Ads.Where(ad => !ad.DisableInterval && !ad.onDead && !ad.onlySpec).ToList(),
            AdType.Spectator => _config.Ads.Where(ad => !ad.DisableInterval && ad.onlySpec).ToList(),
            AdType.OnDead => _config.Ads.Where(ad => !ad.DisableInterval && ad.onDead).ToList(),
            _ => new List<AdConfig>()
        };
    }

    private List<CCSPlayerController> GetTargetPlayersForAdType(AdType adType, AdConfig ad)
    {
        var validPlayers = _playerManager.GetValidPlayers().ToList();

        return adType switch
        {
            AdType.OnDead => validPlayers.Where(p => !p.PawnIsAlive && p.Team != CsTeam.Spectator).ToList(),
            AdType.Spectator => validPlayers.Where(p => p.Team == CsTeam.Spectator).ToList(),
            _ => FilterPlayersForDisplayType(validPlayers, ad.DisplayType)
        };
    }

    private List<CCSPlayerController> FilterPlayersForDisplayType(List<CCSPlayerController> players, DisplayType displayType)
    {
        return displayType switch
        {
            DisplayType.Screen => players,
            _ => players
        };
    }

    public bool CanSendAd(AdConfig ad)
    {
        if (ad.DisableInterval)
            return false;

        if (!ad.HasValidMessage())
            return false;

        if (!_lastAdTimes.ContainsKey(ad))
        {
            _lastAdTimes[ad] = DateTime.MinValue;
        }

        if (!IsMapValid(ad))
            return false;

        if (!IsWarmupStateValid(ad))
            return false;

        return IsIntervalElapsed(ad);
    }

    private void SendAdToPlayers(AdConfig ad, List<CCSPlayerController> targetPlayers)
    {
        try
        {
            if (!targetPlayers.Any()) return;

            int playersReached = 0;
            foreach (var player in targetPlayers)
            {
                if (ShouldSendAdToPlayer(player, ad))
                {
                    SendAdToPlayer(player, ad);
                    playersReached++;
                }
            }

            _lastAdTimes[ad] = DateTime.Now;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error sending ad '{GetAdPreview(ad)}': {ex.Message}");
        }
    }

    public void SendOnDeadAds(CCSPlayerController? deadPlayer)
    {
        if (deadPlayer?.IsValidPlayer() != true || !deadPlayer.PawnIsAlive)
        {
            return;
        }

        if (deadPlayer.Team == CsTeam.Spectator)
        {
            return;
        }

        try
        {
            var immediateOnDeadAds = _config.Ads.Where(ad => ad.onDead && ad.DisableInterval).ToList();
            foreach (var ad in immediateOnDeadAds)
            {
                if (ShouldSendAdToPlayer(deadPlayer, ad))
                {
                    SendAdToPlayer(deadPlayer, ad);
                }
            }

            var intervalOnDeadAds = _config.Ads.Where(ad => ad.onDead && !ad.DisableInterval).ToList();
            foreach (var ad in intervalOnDeadAds)
            {
                if (CanSendAd(ad) && ShouldSendAdToPlayer(deadPlayer, ad))
                {
                    SendAdToPlayer(deadPlayer, ad);
                    _lastAdTimes[ad] = DateTime.Now;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error sending on-dead ads to player {deadPlayer?.PlayerName ?? "Unknown"}: {ex.Message}");
        }
    }

    private bool ShouldSendAdToPlayer(CCSPlayerController player, AdConfig ad)
    {
        try
        {
            if (!player.CanViewMessage(ad.ViewFlag, ad.ExcludeFlag))
            {
                return false;
            }

            if (ad.onlySpec && player.Team != CsTeam.Spectator)
            {
                return false;
            }

            if (!IsMapValid(ad))
            {
                return false;
            }

            if (!IsWarmupStateValid(ad))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error checking if ad '{GetAdPreview(ad)}' should be sent to player {player.PlayerName ?? "Unknown"}: {ex.Message}");
            return false;
        }
    }

    private void InitializeIntervals()
    {
        foreach (var ad in _config.Ads)
        {
            ad.Interval = ad.GetEffectiveInterval(_config.GlobalInterval);
        }
    }

    private void InitializeAdTimes()
    {
        var now = DateTime.Now;
        foreach (var ad in _config.Ads)
        {
            if (!_lastAdTimes.ContainsKey(ad))
            {
                _lastAdTimes[ad] = now.AddSeconds(-(ad.Interval + 10));
            }
        }
    }

    private bool IsMapValid(AdConfig ad)
    {
        return _currentMap.MapMatches(ad.Map);
    }

    private bool IsWarmupStateValid(AdConfig ad)
    {
        bool isWarmup = _gameRules?.WarmupPeriod ?? false;
        if (ad.OnlyInWarmup)
            return isWarmup;

        return true;
    }

    private bool IsIntervalElapsed(AdConfig ad)
    {
        if (!_lastAdTimes.ContainsKey(ad))
        {
            _lastAdTimes[ad] = DateTime.Now.AddSeconds(-(ad.Interval + 10));
            return true;
        }

        var timeSinceLastAd = DateTime.Now - _lastAdTimes[ad];
        var secondsSinceLastMessage = (int)timeSinceLastAd.TotalSeconds;

        return secondsSinceLastMessage >= ad.Interval;
    }

    private string GetAdPreview(AdConfig ad)
    {
        string message = ad.GetMessage();

        if (message.Length > 50)
        {
            message = message.Substring(0, 47) + "...";
        }

        return $"'{message}' (Interval: {ad.Interval}s, DisplayType: {ad.DisplayType})";
    }

    private void SendAdToPlayer(CCSPlayerController player, AdConfig ad)
    {
        try
        {
            if (!ad.HasValidMessage())
            {
                return;
            }

            Server.NextFrame(async () =>
            {
                try
                {
                    string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
                    string formattedMessage;

                    if (_config.UseMultiLang)
                    {
                        Models.PlayerInfo playerInfo;

                        if (_playerManager.NeedsCountryUpdate(player.SteamID))
                        {
                            playerInfo = await _playerManager.GetOrCreatePlayerInfoAsync(player, _ipQueryService);
                        }
                        else
                        {
                            playerInfo = _playerManager.GetBasicPlayerInfo(player);
                        }

                        formattedMessage = _messageFormatter.FormatAdMessage(ad, playerInfo, formattedPrefix);
                    }
                    else
                    {
                        formattedMessage = _messageFormatter.FormatAdMessage(ad, player.PlayerName ?? "Unknown", "", formattedPrefix);
                    }

                    if (string.IsNullOrWhiteSpace(formattedMessage))
                    {
                        return;
                    }

                    DisplayType effectiveDisplayType = ad.DisplayType;
                    if (ad.DisplayType == DisplayType.Screen && !player.PawnIsAlive)
                    {
                        effectiveDisplayType = DisplayType.Chat;
                    }

                    _playerManager.SendMessageToPlayer(player, formattedMessage, effectiveDisplayType);

                    string soundToPlay = ad.PlaySoundName ?? _config.GlobalPlaySound ?? string.Empty;
                    if (!ad.DisableSound && !string.IsNullOrWhiteSpace(soundToPlay))
                    {
                        _playerManager.PlaySoundToPlayer(player, soundToPlay);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutomaticAds] Error in SendAdToPlayer NextFrame: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error sending ad to player {player.PlayerName ?? "Unknown"}: {ex.Message}");
        }
    }
}