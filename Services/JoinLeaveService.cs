using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger _logger;

    public JoinLeaveService(BaseConfigs config, MessageFormatter messageFormatter, PlayerManager playerManager, IIPQueryService ipQueryService, TimerManager timerManager, ILogger logger)
    {
        _config = config;
        _messageFormatter = messageFormatter;
        _playerManager = playerManager;
        _ipQueryService = ipQueryService;
        _timerManager = timerManager;
        _logger = logger;
    }

    public void HandlePlayerJoin(CCSPlayerController player)
    {
        if (!_config.EnableJoinLeaveMessages)
            return;

        if (!ShouldProcessJoinLeaveMessage(player))
            return;

        ulong steamId;
        string? playerName = null;
        string playerIp = string.Empty;

        try
        {
            steamId = player.SteamID;
            playerName = player.PlayerName;
            playerIp = player.GetPlayerIpAddress();
            _logger.LogDebug("HandlePlayerJoin: steamId={SteamId}, name={PlayerName}, ip={PlayerIp}", steamId, playerName, playerIp);
        }
        catch
        {
            return;
        }

        if (_processedJoins.Contains(steamId))
        {
            _logger.LogDebug("HandlePlayerJoin: Already processed join for {SteamId}", steamId);
            return;
        }

        _processedJoins.Add(steamId);

        var joinConfig = _config.JoinLeave.FirstOrDefault();
        if (joinConfig == null || !joinConfig.HasValidJoinMessage())
        {
            _processedJoins.Remove(steamId);
            return;
        }

        try
        {
            string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
            PlayerInfo joiningPlayerInfo;

            if (_config.UseMultiLang)
            {
                joiningPlayerInfo = _playerManager.GetOrCreatePlayerInfo(player, _ipQueryService);
            }
            else
            {
                joiningPlayerInfo = _playerManager.GetBasicPlayerInfo(player);
                joiningPlayerInfo.CountryCode = Utils.Constants.ErrorMessages.Unknown;
                joiningPlayerInfo.CountryName = Utils.Constants.ErrorMessages.Unknown;
            }

            _logger.LogDebug("HandlePlayerJoin: Player {PlayerName} country={CountryCode}", playerName, joiningPlayerInfo.CountryCode);

            Server.NextFrame(() =>
            {
                try
                {
                    if (player.IsValidPlayer())
                    {
                        var validPlayers = _playerManager.GetValidPlayers();

                        foreach (var targetPlayer in validPlayers)
                        {
                            try
                            {
                                string joinMessage;

                                if (_config.UseMultiLang)
                                {
                                    var targetPlayerInfo = _playerManager.GetBasicPlayerInfo(targetPlayer);

                                    var messagePlayerInfo = new Models.PlayerInfo
                                    {
                                        Name = joiningPlayerInfo.Name,
                                        SteamId = joiningPlayerInfo.SteamId,
                                        IpAddress = joiningPlayerInfo.IpAddress,
                                        CountryCode = joiningPlayerInfo.CountryCode,
                                        CountryName = joiningPlayerInfo.CountryName
                                    };

                                    string targetLanguage = _messageFormatter.GetLanguageFromCountryCode(targetPlayerInfo.CountryCode);
                                    string message = joinConfig.GetJoinMessage(targetLanguage);
                                    if (string.IsNullOrWhiteSpace(message))
                                    {
                                        message = joinConfig.GetJoinMessage("en");
                                    }

                                    if (!string.IsNullOrWhiteSpace(message))
                                    {
                                        joinMessage = _messageFormatter.FormatMessageWithPlayerInfo(message, messagePlayerInfo, formattedPrefix);
                                        targetPlayer.PrintToChat(joinMessage);
                                    }
                                }
                                else
                                {
                                    joinMessage = _messageFormatter.FormatJoinLeaveMessage(joinConfig, joiningPlayerInfo, formattedPrefix, true);
                                    if (!string.IsNullOrWhiteSpace(joinMessage))
                                    {
                                        targetPlayer.PrintToChat(joinMessage);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error sending join message to player {PlayerName}", targetPlayer.PlayerName ?? "Unknown");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in HandlePlayerJoin NextFrame");
                }
                finally
                {
                    _timerManager.AddTimer(5.0f, () => _processedJoins.Remove(steamId));
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandlePlayerJoin: Player '{PlayerName}'", playerName ?? "Unknown");
            _processedJoins.Remove(steamId);
        }
    }

    public void HandlePlayerLeave(CCSPlayerController player)
    {
        if (!_config.EnableJoinLeaveMessages)
            return;

        if (!ShouldProcessJoinLeaveMessage(player))
            return;

        ulong steamId;
        string? playerName = null;
        string playerIp = string.Empty;

        try
        {
            steamId = player.SteamID;
            playerName = player.PlayerName;
            playerIp = player.GetPlayerIpAddress();
            _logger.LogDebug("HandlePlayerLeave: steamId={SteamId}, name={PlayerName}", steamId, playerName);
        }
        catch
        {
            return;
        }

        if (_processedLeaves.Contains(steamId))
        {
            _logger.LogDebug("HandlePlayerLeave: Already processed leave for {SteamId}", steamId);
            return;
        }

        _processedLeaves.Add(steamId);

        var leaveConfig = _config.JoinLeave.FirstOrDefault();
        if (leaveConfig == null || !leaveConfig.HasValidLeaveMessage())
        {
            _processedLeaves.Remove(steamId);
            return;
        }

        try
        {
            string formattedPrefix = _messageFormatter.FormatMessage(_config.ChatPrefix);
            PlayerInfo leavingPlayerInfo;

            if (_config.UseMultiLang)
            {
                leavingPlayerInfo = _playerManager.GetOrCreatePlayerInfo(player, _ipQueryService);
            }
            else
            {
                leavingPlayerInfo = _playerManager.GetBasicPlayerInfo(player);
                leavingPlayerInfo.CountryCode = Utils.Constants.ErrorMessages.Unknown;
                leavingPlayerInfo.CountryName = Utils.Constants.ErrorMessages.Unknown;
            }

            Server.NextFrame(() =>
            {
                try
                {
                    var validPlayers = _playerManager.GetValidPlayers().Where(p => p.SteamID != steamId).ToList();
                    foreach (var targetPlayer in validPlayers)
                    {
                        try
                        {
                            string leaveMessage;

                            if (_config.UseMultiLang)
                            {
                                var targetPlayerInfo = _playerManager.GetBasicPlayerInfo(targetPlayer);
                                string targetLanguage = _messageFormatter.GetLanguageFromCountryCode(targetPlayerInfo.CountryCode);
                                string message = leaveConfig.GetLeaveMessage(targetLanguage);
                                if (string.IsNullOrWhiteSpace(message))
                                {
                                    message = leaveConfig.GetLeaveMessage("en");
                                }

                                if (!string.IsNullOrWhiteSpace(message))
                                {
                                    leaveMessage = _messageFormatter.FormatMessageWithPlayerInfo(message, leavingPlayerInfo, formattedPrefix);
                                    targetPlayer.PrintToChat(leaveMessage);
                                }
                            }
                            else
                            {
                                leaveMessage = _messageFormatter.FormatJoinLeaveMessage(leaveConfig, leavingPlayerInfo, formattedPrefix, false);
                                if (!string.IsNullOrWhiteSpace(leaveMessage))
                                {
                                    targetPlayer.PrintToChat(leaveMessage);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending leave message to player {PlayerName}", targetPlayer.PlayerName ?? "Unknown");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in HandlePlayerLeave NextFrame");
                }
                finally
                {
                    _processedLeaves.Remove(steamId);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandlePlayerLeave: Player '{PlayerName}'", playerName ?? "Unknown");
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
                _logger.LogError("Error processing player disconnect: Player object is invalid.");
            }
        }
    }

    private bool ShouldProcessJoinLeaveMessage(CCSPlayerController player)
    {
        return _config.JoinLeave.Any() && player.IsValidPlayer();
    }
}