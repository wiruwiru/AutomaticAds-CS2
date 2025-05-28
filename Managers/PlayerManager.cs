using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using AutomaticAds.Models;
using AutomaticAds.Config;
using AutomaticAds.Utils;

namespace AutomaticAds.Managers;

public class PlayerManager
{
    public List<CCSPlayerController> GetValidPlayers()
    {
        return Utilities.GetPlayers().GetValidPlayers();
    }

    public PlayerInfo CreatePlayerInfo(CCSPlayerController player)
    {
        return new PlayerInfo
        {
            Name = player.PlayerName ?? string.Empty,
            SteamId = player.SteamID.ToString(),
            IpAddress = player.GetPlayerIpAddress()
        };
    }

    public async Task<PlayerInfo> CreatePlayerInfoWithCountryAsync(CCSPlayerController player, Services.IIPQueryService ipQueryService)
    {
        var playerInfo = CreatePlayerInfo(player);

        if (!string.IsNullOrEmpty(playerInfo.IpAddress))
        {
            playerInfo.Country = await ipQueryService.GetCountryAsync(playerInfo.IpAddress);
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
                player.PrintToCenterHtml(message);
                break;
            default:
                player.PrintToChat(message);
                break;
        }
    }
}