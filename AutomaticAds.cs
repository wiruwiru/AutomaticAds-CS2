using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using AutomaticAds.Config;
using AutomaticAds.Services;
using AutomaticAds.Managers;
using AutomaticAds.Utils;

namespace AutomaticAds;

[MinimumApiVersion(290)]
public class AutomaticAdsBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    public override string ModuleName => "AutomaticAds";
    public override string ModuleVersion => "1.2.0";
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
        _messageFormatter = new MessageFormatter();
        _timerManager = new TimerManager(this);
        _playerManager = new PlayerManager();
        _ipQueryService = new IPQueryService();

        _adService = new AdService(Config, _messageFormatter, _timerManager, _playerManager);
        _welcomeService = new WelcomeService(Config, _messageFormatter, _timerManager, _playerManager);
        _joinLeaveService = new JoinLeaveService(Config, _messageFormatter, _playerManager, _ipQueryService, _timerManager);
    }

    private void RegisterEventHandlers()
    {
        RegisterListener<Listeners.OnMapStart>(mapName =>
        {
            _currentMap = mapName;
            _adService?.SetCurrentMap(mapName);

            Server.NextFrame(() =>
            {
                _gameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                    .FirstOrDefault()?.GameRules;
                _adService?.SetGameRules(_gameRulesProxy);
            });
        });

        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);

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
                Server.ExecuteCommand("css_plugins reload AutomaticAds");
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

    private void HandleTriggerCommand(CCSPlayerController? player, BaseConfigs.AdConfig ad)
    {
        if (!player.IsValidPlayer())
            return;

        string formattedPrefix = _messageFormatter!.FormatMessage(Config.ChatPrefix);
        string formattedMessage = _messageFormatter.FormatMessage(ad.Message, player!.PlayerName, formattedPrefix);

        _playerManager!.SendMessageToPlayer(player, formattedMessage);

        string soundToPlay = ad.PlaySoundName ?? Config.GlobalPlaySound ?? string.Empty;
        if (!ad.DisableSound && !string.IsNullOrWhiteSpace(soundToPlay))
        {
            _playerManager.PlaySoundToPlayer(player, soundToPlay);
        }
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

    [GameEventHandler]
    public HookResult OnPlayerFullConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid?.IsValidPlayer() != true)
            return HookResult.Continue;

        _welcomeService?.SendWelcomeMessage(@event.Userid);

        return HookResult.Continue;
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

        _joinLeaveService?.HandlePlayerLeave(player!);

        _welcomeService?.OnPlayerDisconnect(player!);
        _joinLeaveService?.OnPlayerDisconnect(player!);

        return HookResult.Continue;
    }

    private void OnClientPutInServer(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (!player.IsValidPlayer())
            return;

        _joinLeaveService?.HandlePlayerJoin(player!);
    }

    public override void Unload(bool hotReload)
    {
        _timerManager?.KillAllTimers();
    }
}