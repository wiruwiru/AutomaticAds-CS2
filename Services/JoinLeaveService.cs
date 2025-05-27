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
        if (!ShouldProcessJoinLeaveMessage(player))
            return;

        if (_processedJoins.Contains(player.SteamID))
            return;

        _processedJoins.Add(player.SteamID);

        var joinConfig = _config.JoinLeave.FirstOrDefault();
        if (joinConfig == null)
        {
            _processedJoins.Remove(player.SteamID);
            return;
        }

        try
        {
            var playerInfo = await _playerManager.CreatePlayerInfoWithCountryAsync(player, _ipQueryService);

            Server.NextFrame(() =>
            {
                if (player.IsValidPlayer())
                {
                    string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
                    string joinMessage = _messageFormatter.FormatMessageWithPlayerInfo(joinConfig.JoinMessage, playerInfo, formattedPrefix);
                    Server.PrintToChatAll(joinMessage);
                }

                _timerManager.AddTimer(5.0f, () => _processedJoins.Remove(player.SteamID));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in HandlePlayerJoin: {ex.Message}");
            _processedJoins.Remove(player.SteamID);
        }
    }

    public async void HandlePlayerLeave(CCSPlayerController player)
    {
        if (!ShouldProcessJoinLeaveMessage(player))
            return;

        if (_processedLeaves.Contains(player.SteamID))
            return;

        _processedLeaves.Add(player.SteamID);

        var leaveConfig = _config.JoinLeave.FirstOrDefault();
        if (leaveConfig == null)
        {
            _processedLeaves.Remove(player.SteamID);
            return;
        }

        try
        {
            var playerInfo = await _playerManager.CreatePlayerInfoWithCountryAsync(player, _ipQueryService);

            Server.NextFrame(() =>
            {
                string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
                string leaveMessage = _messageFormatter.FormatMessageWithPlayerInfo(leaveConfig.LeaveMessage, playerInfo, formattedPrefix);
                Server.PrintToChatAll(leaveMessage);

                _processedLeaves.Remove(player.SteamID);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in HandlePlayerLeave: {ex.Message}");
            _processedLeaves.Remove(player.SteamID);
        }
    }

    public void OnPlayerDisconnect(CCSPlayerController player)
    {
        if (player?.IsValid == true)
        {
            _processedJoins.Remove(player.SteamID);
            _processedLeaves.Remove(player.SteamID);
        }
    }

    private bool ShouldProcessJoinLeaveMessage(CCSPlayerController player)
    {
        return _config.EnableJoinLeaveMessages &&
               _config.JoinLeave.Any() &&
               player.IsValidPlayer();
    }
}