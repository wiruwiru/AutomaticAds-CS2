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
    private string _currentMap = string.Empty;
    private CCSGameRules? _gameRules;

    private List<BaseConfigs.AdConfig> _intervalAds = new();
    private List<BaseConfigs.AdConfig> _onDeadAds = new();
    private List<BaseConfigs.AdConfig> _onlySpecAds = new();
    private readonly Dictionary<BaseConfigs.AdConfig, DateTime> _lastSpecAdTimes = new();

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

        StartSpectatorAdvertising();
    }

    public bool CanSendAd(BaseConfigs.AdConfig ad)
    {
        if (ad.DisableInterval || ad.onDead || ad.onlySpec)
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

    public void SendSpectatorAds()
    {
        var spectatorPlayers = _playerManager.GetValidPlayers()
            .Where(p => p.Team == CsTeam.Spectator)
            .ToList();

        if (!spectatorPlayers.Any())
        {
            return;
        }

        foreach (var ad in _onlySpecAds)
        {
            if (!CanSendSpecAd(ad))
            {
                continue;
            }

            foreach (var player in spectatorPlayers)
            {
                if (ShouldSendAdToPlayer(player, ad))
                {
                    SendAdToPlayer(player, ad);
                }
            }

            _lastSpecAdTimes[ad] = DateTime.Now;
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
        _onlySpecAds = _config.Ads.Where(ad => ad.onlySpec).ToList();
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

        foreach (var ad in _config.Ads.Where(ad => ad.onlySpec))
        {
            if (!_lastSpecAdTimes.ContainsKey(ad))
            {
                _lastSpecAdTimes[ad] = DateTime.MinValue;
            }
        }
    }

    private void StartIntervalBasedAdvertising()
    {
        foreach (var ad in _intervalAds)
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

    private void StartSpectatorAdvertising()
    {
        if (!_onlySpecAds.Any())
        {
            return;
        }

        _timerManager.AddTimer(5.0f, () =>
        {
            SendSpectatorAds();
        }, TimerFlags.REPEAT);
    }

    private void ScheduleNextAd()
    {
        if (!_intervalAds.Any())
            return;

        var currentAd = _intervalAds[_currentAdIndex];
        float interval = currentAd.Interval;

        var timer = _timerManager.AddTimer(interval, () =>
        {
            if (CanSendAd(currentAd))
            {
                SendAdToPlayers(currentAd);
            }

            _currentAdIndex = (_currentAdIndex + 1) % _intervalAds.Count;
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

    private bool CanSendSpecAd(BaseConfigs.AdConfig ad)
    {
        if (!IsMapValid(ad))
        {
            return false;
        }

        if (!IsWarmupStateValid(ad))
        {
            return false;
        }

        if (!_lastSpecAdTimes.ContainsKey(ad))
        {
            _lastSpecAdTimes[ad] = DateTime.Now;
            return true;
        }
        double secondsSinceLastMessage = (DateTime.Now - _lastSpecAdTimes[ad]).TotalSeconds;
        bool intervalOk = secondsSinceLastMessage >= ad.Interval;

        return intervalOk;
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