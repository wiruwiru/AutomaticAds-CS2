using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using AutomaticAds.Models;
using AutomaticAds.Config;
using AutomaticAds.Utils;

namespace AutomaticAds.Managers;

public class PlayerManager
{
    private readonly AutomaticAdsBase? _plugin;

    public PlayerManager(AutomaticAdsBase? plugin = null)
    {
        _plugin = plugin;
    }

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
            IpAddress = player.GetPlayerIpAddress(),
            CountryCode = _plugin?.Config?.DefaultLanguage ?? "en"
        };
    }

    public async Task<PlayerInfo> CreatePlayerInfoWithCountryAsync(CCSPlayerController player, Services.IIPQueryService ipQueryService)
    {
        var playerInfo = CreatePlayerInfo(player);

        if (!string.IsNullOrEmpty(playerInfo.IpAddress))
        {
            string countryCode = await ipQueryService.GetCountryCodeAsync(playerInfo.IpAddress);

            if (countryCode != Utils.Constants.ErrorMessages.CountryCodeError)
            {
                playerInfo.CountryCode = countryCode;
                playerInfo.CountryName = CountryMapping.GetCountryName(countryCode);
            }
            else
            {
                playerInfo.CountryCode = Utils.Constants.ErrorMessages.Unknown;
                playerInfo.CountryName = Utils.Constants.ErrorMessages.Unknown;
            }
        }
        else
        {
            playerInfo.CountryCode = Utils.Constants.ErrorMessages.Unknown;
            playerInfo.CountryName = Utils.Constants.ErrorMessages.Unknown;
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
                _plugin?.StartCenterHtmlMessage(player, message);
                break;
            default:
                player.PrintToChat(message);
                break;
        }
    }
}