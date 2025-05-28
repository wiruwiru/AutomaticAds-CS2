using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
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
    private int _currentSpecAdIndex = 0;
    private string _currentMap = string.Empty;
    private CCSGameRules? _gameRules;

    private List<BaseConfigs.AdConfig> _intervalAds = new();
    private List<BaseConfigs.AdConfig> _onDeadAds = new();
    private List<BaseConfigs.AdConfig> _onlySpecAds = new();

    public AdService(BaseConfigs config, MessageFormatter messageFormatter, TimerManager timerManager, PlayerManager playerManager)
    {
        _config = config;
        _messageFormatter = messageFormatter;
        _timerManager = timerManager;
        _playerManager = playerManager;
        InitializeAdTimes();
        SeparateAdTypes();
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
            _intervalAds = _config.Ads.Where(ad => !ad.DisableInterval && !ad.onDead && !ad.onlySpec).ToList();
            ScheduleNextAd();
        }
        else
        {
            StartIntervalBasedAdvertising();
        }

        if (_config.SendAdsInOrder)
        {
            _onlySpecAds = _config.Ads.Where(ad => !ad.DisableInterval && ad.onlySpec).ToList();
            ScheduleNextSpecAd();
        }
        else
        {
            StartIntervalBasedSpecAdvertising();
        }
    }

    public bool CanSendAd(BaseConfigs.AdConfig ad)
    {
        if (ad.DisableInterval || ad.onDead)
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

    public void SendAdToPlayers(BaseConfigs.AdConfig ad)
    {
        List<CCSPlayerController> targetPlayers;

        if (ad.onlySpec)
        {
            targetPlayers = _playerManager.GetValidPlayers()
                .Where(p => p.Team == CsTeam.Spectator)
                .ToList();
        }
        else
        {
            targetPlayers = _playerManager.GetValidPlayers();
        }

        if (!targetPlayers.Any())
            return;

        foreach (var player in targetPlayers)
        {
            if (ShouldSendAdToPlayer(player, ad))
            {
                SendAdToPlayer(player, ad);
            }
        }

        _lastAdTimes[ad] = DateTime.Now;
    }

    public void SendOnDeadAds(CCSPlayerController? deadPlayer)
    {
        if (deadPlayer?.IsValidPlayer() != true)
            return;

        foreach (var ad in _onDeadAds)
        {
            if (ShouldSendAdToPlayer(deadPlayer, ad))
            {
                SendAdToPlayer(deadPlayer, ad);
            }
        }
    }

    private bool ShouldSendAdToPlayer(CCSPlayerController player, BaseConfigs.AdConfig ad)
    {
        if (!player.CanViewMessage(ad.ViewFlag, ad.ExcludeFlag))
        {
            return false;
        }

        if (ad.onlySpec && player.Team != CsTeam.Spectator)
        {
            return false;
        }

        if (ad.onDead && !IsMapValid(ad))
        {
            return false;
        }

        if (ad.onDead && !IsWarmupStateValid(ad))
        {
            return false;
        }

        return true;
    }

    private void SeparateAdTypes()
    {
        _intervalAds = _config.Ads.Where(ad => !ad.DisableInterval && !ad.onDead && !ad.onlySpec).ToList();
        _onDeadAds = _config.Ads.Where(ad => ad.onDead).ToList();
        _onlySpecAds = _config.Ads.Where(ad => !ad.DisableInterval && ad.onlySpec).ToList();
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

    private void StartIntervalBasedAdvertising()
    {
        foreach (var ad in _intervalAds)
        {
            _timerManager.AddTimer(Math.Max(1.0f, ad.Interval / 10.0f), () =>
            {
                if (CanSendAd(ad))
                {
                    SendAdToPlayers(ad);
                }
            }, TimerFlags.REPEAT);
        }
    }

    private void StartIntervalBasedSpecAdvertising()
    {
        foreach (var ad in _onlySpecAds)
        {
            _timerManager.AddTimer(Math.Max(1.0f, ad.Interval / 10.0f), () =>
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
        if (!_intervalAds.Any())
            return;

        var currentAd = _intervalAds[_currentAdIndex];
        float interval = currentAd.Interval;

        var timer = _timerManager.AddTimer(interval, () =>
        {
            if (IsMapValid(currentAd) && IsWarmupStateValid(currentAd))
            {
                SendAdToPlayers(currentAd);
            }

            _currentAdIndex = (_currentAdIndex + 1) % _intervalAds.Count;
            ScheduleNextAd();
        });

        _timerManager.SetAdTimer(timer);
    }

    private void ScheduleNextSpecAd()
    {
        if (!_onlySpecAds.Any())
            return;

        var currentSpecAd = _onlySpecAds[_currentSpecAdIndex];
        float interval = currentSpecAd.Interval;

        var timer = _timerManager.AddTimer(interval, () =>
        {
            if (IsMapValid(currentSpecAd) && IsWarmupStateValid(currentSpecAd))
            {
                SendAdToPlayers(currentSpecAd);
            }

            _currentSpecAdIndex = (_currentSpecAdIndex + 1) % _onlySpecAds.Count;
            ScheduleNextSpecAd();
        });

        _timerManager.SetSpecAdTimer(timer);
    }

    private bool IsMapValid(BaseConfigs.AdConfig ad)
    {
        return _currentMap.MapMatches(ad.Map);
    }

    private bool IsWarmupStateValid(BaseConfigs.AdConfig ad)
    {
        bool isWarmup = _gameRules?.WarmupPeriod ?? false;
        if (ad.OnlyInWarmup)
            return isWarmup;

        return !isWarmup;
    }

    private bool IsIntervalElapsed(BaseConfigs.AdConfig ad)
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