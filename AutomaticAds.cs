using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using AutomaticAds.Config;
using AutomaticAds.Config.Models;
using AutomaticAds.Services;
using AutomaticAds.Managers;
using AutomaticAds.Utils;

namespace AutomaticAds;

[MinimumApiVersion(290)]
public class AutomaticAdsBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    public override string ModuleName => "AutomaticAds";
    public override string ModuleVersion => "1.2.6";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Send automatic messages to the chat and play a sound alert for users to see the message.";

    // Services and Managers
    private AdService? _adService;
    private WelcomeService? _welcomeService;
    private JoinLeaveService? _joinLeaveService;
    private IIPQueryService? _ipQueryService;
    private TimerManager? _timerManager;
    private PlayerManager? _playerManager;
    private MessageFormatter? _messageFormatter;

    // Game state
    private string _currentMap = string.Empty;
    private CCSGameRules? _gameRulesProxy;

    // CenterHtml message tracking and configuration
    private readonly Dictionary<int, DateTime> _centerHtmlStartTimes = new();
    private readonly Dictionary<int, string> _activeCenterHtmlMessages = new();
    private readonly Dictionary<int, DateTime> _lastCenterHtmlUpdateTimes = new();

    public required BaseConfigs Config { get; set; }

    public override void Load(bool hotReload)
    {
        InitializeServices();
        RegisterEventHandlers();
        RegisterCommands();

        if (hotReload)
        {
            _currentMap = Server.MapName;
            _adService?.SetCurrentMap(_currentMap);
            _messageFormatter?.SetCurrentMap(_currentMap);
        }

        if (!string.IsNullOrWhiteSpace(Server.MapName))
        {
            OnMapStart(Server.MapName);
        }
    }

    public void OnConfigParsed(BaseConfigs config)
    {
        ConfigValidator.ValidateConfig(config);
        Config = config;
    }

    private void InitializeServices()
    {
        _messageFormatter = new MessageFormatter(Config);
        _timerManager = new TimerManager(this);
        _playerManager = new PlayerManager(this);
        _ipQueryService = new IPQueryService();

        _adService = new AdService(Config, _messageFormatter, _timerManager, _playerManager, _ipQueryService);
        _welcomeService = new WelcomeService(Config, _messageFormatter, _timerManager, _playerManager);
        _joinLeaveService = new JoinLeaveService(Config, _messageFormatter, _playerManager, _ipQueryService, _timerManager);
    }

    private void RegisterEventHandlers()
    {
        RegisterListener<Listeners.OnMapStart>(mapName =>
        {
            _currentMap = mapName;
            _adService?.SetCurrentMap(mapName);
            _messageFormatter?.SetCurrentMap(mapName);

            Server.NextFrame(() =>
            {
                _gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                    .FirstOrDefault()?.GameRules;
                _adService?.SetGameRules(_gameRulesProxy);
            });
        });

        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnTick>(OnTick);

        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerFullConnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnectPre, HookMode.Pre);
    }

    private void RegisterCommands()
    {
        RegisterReloadCommand();
        RegisterTriggerCommands();
    }

    private void RegisterReloadCommand()
    {
        AddCommand("ads_reload", "Reloads the AutomaticAds plugin configuration.", (player, commandInfo) =>
        {
            if (player == null || commandInfo == null)
                return;

            if (!HasReloadPermission(player))
            {
                SendNoPermissionMessage(player);
                return;
            }

            try
            {
                Server.ExecuteCommand($"css_plugins reload {ModuleName}");
                string formattedPrefix = _messageFormatter!.FormatMessage(Config.ChatPrefix);
                commandInfo.ReplyToCommand($"{formattedPrefix} {Localizer["Reloaded"]}");
            }
            catch (Exception ex)
            {
                string formattedPrefix = _messageFormatter!.FormatMessage(Config.ChatPrefix);
                commandInfo.ReplyToCommand($"{formattedPrefix} {Localizer["FailedToReload"]}: {ex.Message}");
            }
        });
    }

    private void RegisterTriggerCommands()
    {
        foreach (var ad in Config.Ads.Where(ad => ad.TriggerAd?.Any() == true))
        {
            foreach (var command in ad.TriggerAd!)
            {
                AddCommand(command, $"Sends the advertisement '{command}' to the user using the command.",
                    (player, commandInfo) => HandleTriggerCommand(player, ad));
            }
        }
    }

    private void HandleTriggerCommand(CCSPlayerController? player, AdConfig ad)
    {
        if (!player.IsValidPlayer())
            return;

        Server.NextFrame(async () =>
        {
            try
            {
                string formattedPrefix = _messageFormatter!.FormatMessage(Config.ChatPrefix);
                string formattedMessage;

                if (Config.UseMultiLang)
                {
                    Models.PlayerInfo playerInfo;

                    if (_playerManager!.NeedsCountryUpdate(player!.SteamID))
                    {
                        playerInfo = await _playerManager.GetOrCreatePlayerInfoAsync(player!, _ipQueryService);
                    }
                    else
                    {
                        playerInfo = _playerManager.GetBasicPlayerInfo(player!);
                    }

                    formattedMessage = _messageFormatter.FormatAdMessage(ad, playerInfo, formattedPrefix);
                }
                else
                {
                    formattedMessage = _messageFormatter.FormatAdMessage(ad, player?.PlayerName ?? "Unknown", "", formattedPrefix);
                }

                if (!string.IsNullOrWhiteSpace(formattedMessage))
                {
                    _playerManager!.SendMessageToPlayer(player!, formattedMessage, ad.DisplayType);

                    string soundToPlay = ad.PlaySoundName ?? Config.GlobalPlaySound ?? string.Empty;
                    if (!ad.DisableSound && !string.IsNullOrWhiteSpace(soundToPlay))
                    {
                        _playerManager.PlaySoundToPlayer(player!, soundToPlay);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AutomaticAds] Error in HandleTriggerCommand: {ex.Message}");
            }
        });
    }

    private bool HasReloadPermission(CCSPlayerController player)
    {
        var permissionValidator = new RequiresPermissions(Utils.Constants.RootPermission);
        return permissionValidator.CanExecuteCommand(player);
    }

    private void SendNoPermissionMessage(CCSPlayerController player)
    {
        string formattedPrefix = _messageFormatter!.FormatMessage(Config.ChatPrefix);
        _playerManager!.SendMessageToPlayer(player, $"{formattedPrefix} {Localizer["NoPermissions"]}");
    }

    private void OnMapStart(string mapName)
    {
        _adService?.StartAdvertising();
    }

    private void OnTick()
    {
        var currentTime = DateTime.Now;
        var playersToRemove = new List<int>();

        foreach (var player in Utilities.GetPlayers())
        {
            if (!player.IsValid || player.IsBot || !player.UserId.HasValue)
            {
                continue;
            }

            int playerId = player.UserId.Value;

            if (_activeCenterHtmlMessages.ContainsKey(playerId) && _centerHtmlStartTimes.ContainsKey(playerId))
            {
                var startTime = _centerHtmlStartTimes[playerId];
                var elapsedTime = (currentTime - startTime).TotalSeconds;

                if (elapsedTime >= Config.centerHtmlDisplayTime)
                {
                    playersToRemove.Add(playerId);
                    continue;
                }

                if (!_lastCenterHtmlUpdateTimes.ContainsKey(playerId) ||
                    (currentTime - _lastCenterHtmlUpdateTimes[playerId]).TotalMilliseconds >= 40)
                {
                    string message = _activeCenterHtmlMessages[playerId];
                    player.PrintToCenterHtml(message);
                    _lastCenterHtmlUpdateTimes[playerId] = currentTime;
                }
            }
        }

        foreach (int playerId in playersToRemove)
        {
            _activeCenterHtmlMessages.Remove(playerId);
            _centerHtmlStartTimes.Remove(playerId);
            _lastCenterHtmlUpdateTimes.Remove(playerId);
        }
    }

    public void StartCenterHtmlMessage(CCSPlayerController player, string message)
    {
        if (!player.IsValid || !player.UserId.HasValue)
            return;

        int playerId = player.UserId.Value;
        var currentTime = DateTime.Now;

        _activeCenterHtmlMessages[playerId] = message;
        _centerHtmlStartTimes[playerId] = currentTime;
        _lastCenterHtmlUpdateTimes[playerId] = currentTime;

        player.PrintToCenterHtml(message);
    }

    public void StopCenterHtmlMessage(CCSPlayerController player)
    {
        if (!player.IsValid || !player.UserId.HasValue)
            return;

        int playerId = player.UserId.Value;

        _activeCenterHtmlMessages.Remove(playerId);
        _centerHtmlStartTimes.Remove(playerId);
        _lastCenterHtmlUpdateTimes.Remove(playerId);
    }

    [GameEventHandler]
    public HookResult OnPlayerFullConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid?.IsValidPlayer() != true)
            return HookResult.Continue;

        var player = @event.Userid;

        if (Config.EnableJoinLeaveMessages || Config.UseMultiLang)
        {
            HandlePlayerConnectWithCountryInfo(player);
        }
        else
        {
            HandlePlayerConnectBasic(player);
        }

        return HookResult.Continue;
    }

    private async void HandlePlayerConnectWithCountryInfo(CCSPlayerController player)
    {
        try
        {
            var playerInfo = await _playerManager!.GetOrCreatePlayerInfoAsync(player, _ipQueryService);
            Server.NextFrame(() =>
            {
                try
                {
                    if (player.IsValidPlayer())
                    {
                        if (Config.EnableJoinLeaveMessages)
                        {
                            _joinLeaveService?.HandlePlayerJoin(player);
                        }

                        _welcomeService?.SendWelcomeMessage(player);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutomaticAds] Error in HandlePlayerConnectWithCountryInfo NextFrame: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error in HandlePlayerConnectWithCountryInfo: {ex.Message}");
            HandlePlayerConnectBasic(player);
        }
    }

    private void HandlePlayerConnectBasic(CCSPlayerController player)
    {
        try
        {
            _playerManager!.GetBasicPlayerInfo(player);

            Server.NextFrame(() =>
            {
                try
                {
                    if (player.IsValidPlayer())
                    {
                        _welcomeService?.SendWelcomeMessage(player);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AutomaticAds] Error in HandlePlayerConnectBasic NextFrame: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutomaticAds] Error in HandlePlayerConnectBasic: {ex.Message}");
        }
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnectPre(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (!Config.EnableJoinLeaveMessages || @event == null)
            return HookResult.Continue;

        info.DontBroadcast = true;
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!player.IsValidPlayer())
            return HookResult.Continue;

        StopCenterHtmlMessage(player!);

        _playerManager?.ClearPlayerCache(player!);
        _joinLeaveService?.HandlePlayerLeave(player!);
        _welcomeService?.OnPlayerDisconnect(player!);
        _joinLeaveService?.OnPlayerDisconnect(player!);

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnPlayerDeath(EventPlayerDeath gameEvent, GameEventInfo info)
    {
        var player = gameEvent.Userid;
        if (!player.IsValidPlayer())
            return HookResult.Continue;

        _adService?.SendOnDeadAds(player);
        return HookResult.Continue;
    }

    public override void Unload(bool hotReload)
    {
        _timerManager?.KillAllTimers();
        RemoveListener<Listeners.OnTick>(OnTick);
    }
}