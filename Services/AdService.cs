using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using AutomaticAds.Config;
using AutomaticAds.Config.Models;
using AutomaticAds.Managers;
using AutomaticAds.Utils;

namespace AutomaticAds.Services;

public class AdService
{
    private readonly BaseConfigs _config;
    private readonly MessageFormatter _messageFormatter;
    private readonly TimerManager _timerManager;
    private readonly PlayerManager _playerManager;
    private readonly IIPQueryService _ipQueryService;
    private readonly Dictionary<AdConfig, DateTime> _lastAdTimes = new();
    private int _currentAdIndex = 0;
    private int _currentSpecAdIndex = 0;
    private int _currentOnDeadAdIndex = 0;
    private string _currentMap = string.Empty;
    private CCSGameRules? _gameRules;

    private List<AdConfig> _intervalAds = new();
    private List<AdConfig> _onDeadAds = new();
    private List<AdConfig> _onlySpecAds = new();

    public AdService(BaseConfigs config, MessageFormatter messageFormatter, TimerManager timerManager, PlayerManager playerManager, IIPQueryService ipQueryService)
    {
        _config = config;
        _messageFormatter = messageFormatter;
        _timerManager = timerManager;
        _playerManager = playerManager;
        _ipQueryService = ipQueryService;
        InitializeAdTimes();
        SeparateAdTypes();
        InitializeIntervals();
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

        if (!_config.SendAdsInOrder)
        {
            StartIntervalBasedOnDeadAdvertising();
        }
        else
        {
            _onDeadAds = _config.Ads.Where(ad => !ad.DisableInterval && ad.onDead).ToList();
            if (_onDeadAds.Any())
            {
                ScheduleNextOnDeadAd();
            }
        }
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

    public void SendAdToPlayers(AdConfig ad)
    {
        try
        {
            int playersReached = 0;

            if (ad.onDead)
            {
                var deadPlayers = _playerManager.GetValidPlayers()
                    .Where(p => !p.PawnIsAlive)
                    .ToList();

                if (!deadPlayers.Any())
                {
                    return;
                }

                foreach (var player in deadPlayers)
                {
                    if (ShouldSendAdToPlayer(player, ad))
                    {
                        SendAdToPlayer(player, ad);
                        playersReached++;
                    }
                }
            }
            else
            {
                List<CCSPlayerController> targetPlayers = _playerManager.GetValidPlayers().ToList();

                if (ad.onlySpec)
                {
                    targetPlayers = targetPlayers.Where(p => p.Team == CsTeam.Spectator).ToList();
                }

                foreach (var player in targetPlayers)
                {
                    if (ShouldSendAdToPlayer(player, ad))
                    {
                        SendAdToPlayer(player, ad);
                        playersReached++;
                    }
                }
            }

            _lastAdTimes[ad] = DateTime.Now;

            string adType = ad.onDead ? "on-dead" : ad.onlySpec ? "spectator" : "interval";
            string adPreview = GetAdPreview(ad);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error sending ad '{ad}': {ex.Message}");
        }
    }

    public void SendOnDeadAds(CCSPlayerController? deadPlayer)
    {
        if (deadPlayer?.IsValidPlayer() != true || deadPlayer.PawnIsAlive)
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
                    string adPreview = GetAdPreview(ad);
                }
            }

            var intervalOnDeadAds = _config.Ads.Where(ad => ad.onDead && !ad.DisableInterval).ToList();
            foreach (var ad in intervalOnDeadAds)
            {
                if (CanSendAd(ad) && ShouldSendAdToPlayer(deadPlayer, ad))
                {
                    SendAdToPlayer(deadPlayer, ad);
                    _lastAdTimes[ad] = DateTime.Now;
                    string adPreview = GetAdPreview(ad);
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
            Console.WriteLine($"[AutomaticAds] Error checking if ad '{ad}' should be sent to player {player.PlayerName ?? "Unknown"}: {ex.Message}");
            return false;
        }
    }

    private void SeparateAdTypes()
    {
        _intervalAds = _config.Ads.Where(ad => !ad.DisableInterval && !ad.onDead && !ad.onlySpec).ToList();
        _onDeadAds = _config.Ads.Where(ad => !ad.DisableInterval && ad.onDead).ToList();
        _onlySpecAds = _config.Ads.Where(ad => !ad.DisableInterval && ad.onlySpec).ToList();
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

    private void StartIntervalBasedOnDeadAdvertising()
    {
        foreach (var ad in _onDeadAds)
        {
            _timerManager.AddTimer(Math.Max(1.0f, ad.Interval / 10.0f), () =>
            {
                if (CanSendAd(ad))
                {
                    var deadPlayers = _playerManager.GetValidPlayers().Where(p => !p.PawnIsAlive).ToList();
                    if (deadPlayers.Any())
                    {
                        SendAdToPlayers(ad);
                    }
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

        var nextIndex = (_currentAdIndex + 1) % _intervalAds.Count;
        var nextAd = _intervalAds[nextIndex];
        string nextAdPreview = GetAdPreview(nextAd);

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

        var nextIndex = (_currentSpecAdIndex + 1) % _onlySpecAds.Count;
        var nextSpecAd = _onlySpecAds[nextIndex];
        string nextAdPreview = GetAdPreview(nextSpecAd);

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

    private void ScheduleNextOnDeadAd()
    {
        if (!_onDeadAds.Any())
        {
            return;
        }

        var currentOnDeadAd = _onDeadAds[_currentOnDeadAdIndex];
        var nextIndex = (_currentOnDeadAdIndex + 1) % _onDeadAds.Count;
        var nextOnDeadAd = _onDeadAds[nextIndex];
        string nextAdPreview = GetAdPreview(nextOnDeadAd);

        var timer = _timerManager.AddTimer(currentOnDeadAd.Interval, () =>
        {
            if (!IsMapValid(currentOnDeadAd) || !IsWarmupStateValid(currentOnDeadAd))
            {
                _currentOnDeadAdIndex = (_currentOnDeadAdIndex + 1) % _onDeadAds.Count;
                ScheduleNextOnDeadAd();
                return;
            }

            var deadPlayers = _playerManager.GetValidPlayers()
                .Where(p => !p.PawnIsAlive)
                .ToList();

            if (deadPlayers.Any())
            {
                SendAdToPlayers(currentOnDeadAd);
            }

            _currentOnDeadAdIndex = (_currentOnDeadAdIndex + 1) % _onDeadAds.Count;
            ScheduleNextOnDeadAd();
        });

        _timerManager.SetOnDeadAdTimer(timer);
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

        return !isWarmup;
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

        return $"'{message}' (Interval: {ad.Interval}s)";
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

                    _playerManager.SendMessageToPlayer(player, formattedMessage, ad.DisplayType);

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