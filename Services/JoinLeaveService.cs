using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using AutomaticAds.Config;
using AutomaticAds.Managers;
using AutomaticAds.Models;
using AutomaticAds.Utils;

namespace AutomaticAds.Services;

public class JoinLeaveService
{
    private readonly BaseConfigs _config;
    private readonly MessageFormatter _messageFormatter;
    private readonly PlayerManager _playerManager;
    private readonly IIPQueryService _ipQueryService;
    private readonly HashSet<ulong> _processedJoins = new();
    private readonly HashSet<ulong> _processedLeaves = new();
    private readonly TimerManager _timerManager;

    public JoinLeaveService(BaseConfigs config, MessageFormatter messageFormatter, PlayerManager playerManager, IIPQueryService ipQueryService, TimerManager timerManager)
    {
        _config = config;
        _messageFormatter = messageFormatter;
        _playerManager = playerManager;
        _ipQueryService = ipQueryService;
        _timerManager = timerManager;
    }

    public async void HandlePlayerJoin(CCSPlayerController player)
    {
        if (!_config.EnableJoinLeaveMessages)
            return;

        if (!ShouldProcessJoinLeaveMessage(player))
            return;

        ulong steamId;
        try
        {
            steamId = player.SteamID;
        }
        catch
        {
            return;
        }

        if (_processedJoins.Contains(steamId))
            return;

        _processedJoins.Add(steamId);

        var joinConfig = _config.JoinLeave.FirstOrDefault();
        if (joinConfig == null)
        {
            _processedJoins.Remove(steamId);
            return;
        }

        try
        {
            string playerIp = player.GetPlayerIpAddress();
            var playerInfo = await _playerManager.UpdatePlayerInfoWithCountryAsync(player, _ipQueryService);

            Server.NextFrame(() =>
            {
                try
                {
                    if (player.IsValidPlayer())
                    {
                        string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
                        string joinMessage = _messageFormatter.FormatMessageWithPlayerInfo(joinConfig.JoinMessage, playerInfo, formattedPrefix);
                        Server.PrintToChatAll(joinMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutomaticAds] Error formatting join message: {ex.Message}");
                }
                finally
                {
                    _timerManager.AddTimer(5.0f, () => _processedJoins.Remove(steamId));
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error in HandlePlayerJoin API Query: Player '{player.PlayerName ?? "Unknown"}' - {ex.Message}");
            _processedJoins.Remove(steamId);
        }
    }

    public async void HandlePlayerLeave(CCSPlayerController player)
    {
        if (!_config.EnableJoinLeaveMessages)
            return;

        if (!ShouldProcessJoinLeaveMessage(player))
            return;

        ulong steamId;
        try
        {
            steamId = player.SteamID;
        }
        catch
        {
            return;
        }

        if (_processedLeaves.Contains(steamId))
            return;

        _processedLeaves.Add(steamId);

        var leaveConfig = _config.JoinLeave.FirstOrDefault();
        if (leaveConfig == null)
        {
            _processedLeaves.Remove(steamId);
            return;
        }

        try
        {
            string playerIp = player.GetPlayerIpAddress();
            var playerInfo = await _playerManager.CreatePlayerInfoWithCountryAsync(player, _ipQueryService);

            Server.NextFrame(() =>
            {
                try
                {
                    string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
                    string leaveMessage = _messageFormatter.FormatMessageWithPlayerInfo(leaveConfig.LeaveMessage, playerInfo, formattedPrefix);
                    Server.PrintToChatAll(leaveMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutomaticAds] Error formatting leave message: {ex.Message}");
                }
                finally
                {
                    _processedLeaves.Remove(steamId);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error in HandlePlayerLeave API Query: Player '{player.PlayerName ?? "Unknown"}' - {ex.Message}");
            _processedLeaves.Remove(steamId);
        }
    }

    public void OnPlayerDisconnect(CCSPlayerController player)
    {
        if (player?.IsValid == true)
        {
            try
            {
                ulong steamId = player.SteamID;
                _processedJoins.Remove(steamId);
                _processedLeaves.Remove(steamId);
            }
            catch
            {
                Console.WriteLine("[AutomaticAds] Error processing player disconnect: Player object is invalid.");
            }
        }
    }

    private bool ShouldProcessJoinLeaveMessage(CCSPlayerController player)
    {
        return _config.JoinLeave.Any() && player.IsValidPlayer();
    }
}