using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using AutomaticAds.Config;
using AutomaticAds.Managers;
using AutomaticAds.Utils;

namespace AutomaticAds.Services;

public class AdService
{
    private readonly BaseConfigs _config;
    private readonly MessageFormatter _messageFormatter;
    private readonly TimerManager _timerManager;
    private readonly PlayerManager _playerManager;
    private readonly Dictionary<BaseConfigs.AdConfig, DateTime> _lastAdTimes = new();
    private int _currentAdIndex = 0;
    private string _currentMap = string.Empty;
    private CCSGameRules? _gameRules;

    public AdService(BaseConfigs config, MessageFormatter messageFormatter, TimerManager timerManager, PlayerManager playerManager)
    {
        _config = config;
        _messageFormatter = messageFormatter;
        _timerManager = timerManager;
        _playerManager = playerManager;
        InitializeAdTimes();
    }

    public void SetCurrentMap(string mapName)
    {
        _currentMap = mapName;
    }

    public void SetGameRules(CCSGameRules? gameRules)
    {
        _gameRules = gameRules;
    }

    public void StartAdvertising()
    {
        if (_config.SendAdsInOrder)
        {
            var enabledAds = _config.Ads.Where(ad => !ad.DisableInterval).ToList();
            _config.Ads.Clear();
            _config.Ads.AddRange(enabledAds);
            ScheduleNextAd();
        }
        else
        {
            StartIntervalBasedAdvertising();
        }
    }

    public bool CanSendAd(BaseConfigs.AdConfig ad)
    {
        if (ad.DisableInterval)
            return false;

        if (!_lastAdTimes.ContainsKey(ad))
        {
            _lastAdTimes[ad] = DateTime.MinValue;
            return true;
        }

        if (!IsMapValid(ad))
            return false;

        if (!IsWarmupStateValid(ad))
            return false;

        return IsIntervalElapsed(ad);
    }

    public void SendAdToPlayers(BaseConfigs.AdConfig ad)
    {
        var players = _playerManager.GetValidPlayers();
        if (!players.Any())
            return;

        foreach (var player in players)
        {
            if (player.CanViewMessage(ad.ViewFlag, ad.ExcludeFlag))
            {
                SendAdToPlayer(player, ad);
            }
        }

        _lastAdTimes[ad] = DateTime.Now;
    }

    private void InitializeAdTimes()
    {
        foreach (var ad in _config.Ads)
        {
            if (!_lastAdTimes.ContainsKey(ad))
            {
                _lastAdTimes[ad] = DateTime.MinValue;
            }
        }
    }

    private void StartIntervalBasedAdvertising()
    {
        foreach (var ad in _config.Ads.Where(ad => !ad.DisableInterval))
        {
            _timerManager.AddTimer(1.0f, () =>
            {
                if (CanSendAd(ad))
                {
                    SendAdToPlayers(ad);
                }
            }, TimerFlags.REPEAT);
        }
    }

    private void ScheduleNextAd()
    {
        if (!_config.Ads.Any())
            return;

        var currentAd = _config.Ads[_currentAdIndex];
        float interval = currentAd.Interval;

        var timer = _timerManager.AddTimer(interval, () =>
        {
            if (CanSendAd(currentAd))
            {
                SendAdToPlayers(currentAd);
            }

            _currentAdIndex = (_currentAdIndex + 1) % _config.Ads.Count;
            ScheduleNextAd();
        });

        _timerManager.SetAdTimer(timer);
    }

    private bool IsMapValid(BaseConfigs.AdConfig ad)
    {
        return _currentMap.MapMatches(ad.Map);
    }

    private bool IsWarmupStateValid(BaseConfigs.AdConfig ad)
    {
        bool isWarmup = _gameRules?.WarmupPeriod ?? false;
        return !(ad.OnlyInWarmup && !isWarmup) && !(!ad.OnlyInWarmup && isWarmup);
    }

    private bool IsIntervalElapsed(BaseConfigs.AdConfig ad)
    {
        var secondsSinceLastMessage = (int)(DateTime.Now - _lastAdTimes[ad]).TotalSeconds;
        return secondsSinceLastMessage >= ad.Interval;
    }

    private void SendAdToPlayer(CCSPlayerController player, BaseConfigs.AdConfig ad)
    {
        string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
        string formattedMessage = _messageFormatter.FormatMessage(ad.Message, player.PlayerName, formattedPrefix);

        _playerManager.SendMessageToPlayer(player, formattedMessage);

        string soundToPlay = ad.PlaySoundName ?? _config.GlobalPlaySound ?? string.Empty;
        if (!ad.DisableSound && !string.IsNullOrWhiteSpace(soundToPlay))
        {
            _playerManager.PlaySoundToPlayer(player, soundToPlay);
        }
    }
}