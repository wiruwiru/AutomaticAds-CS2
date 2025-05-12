using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace AutomaticAds;

[MinimumApiVersion(290)]
public class AutomaticAdsBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    public override string ModuleName => "AutomaticAds";
    public override string ModuleVersion => "1.1.3b";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "I send automatic messages to the chat and play a sound alert for users to see the message.";

    private readonly Dictionary<BaseConfigs.AdConfig, DateTime> lastAdTimes = new();
    private readonly List<CounterStrikeSharp.API.Modules.Timers.Timer> timers = new();
    private CounterStrikeSharp.API.Modules.Timers.Timer? adTimer = null;

    private int currentAdIndex = 0;
    private string _currentMap = "";
    private CCSGameRules? _gGameRulesProxy;

    public override void Load(bool hotReload)
    {

        RegisterListener<Listeners.OnMapStart>(mapName =>
        {
            _currentMap = mapName;

            Server.NextFrame(() =>
            {
                _gGameRulesProxy = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                .First().GameRules ?? throw new Exception("Failed to find game rules proxy entity.");
            });
        });

        if (hotReload)
        {
            _currentMap = Server.MapName;
        }

        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));
        RegisterListener<Listeners.OnMapStart>(OnMapStart);

        RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnectPre, HookMode.Pre);

        if (!string.IsNullOrWhiteSpace(Server.MapName))
        {
            OnMapStart(Server.MapName);
        }

        AddCommand("ads_reload", "Reloads the AutomaticAds plugin configuration.", (player, commandInfo) =>
        {

            if (player == null || commandInfo == null) return;
            MessageColorFormatter formatter = new MessageColorFormatter();
            string formattedPrefix = formatter.FormatMessage(Config.ChatPrefix);

            var permissionValidator = new RequiresPermissions("@css/root");
            if (!permissionValidator.CanExecuteCommand(player))
            {
                player.PrintToChat($"{formattedPrefix} {Localizer["NoPermissions"]}");
                return;
            }

            try
            {
                Server.ExecuteCommand("css_plugins reload AutomaticAds");
                commandInfo.ReplyToCommand($"{formattedPrefix} {Localizer["Reloaded"]}");
            }
            catch (Exception ex)
            {
                commandInfo.ReplyToCommand($"{formattedPrefix} {Localizer["FailedToReload"]}: {ex.Message}");
            }
        });

        foreach (var ad in Config.Ads.Where(ad => ad.triggerAd != null && ad.triggerAd.Any()))
        {
            foreach (var command in ad.triggerAd!)
            {
                AddCommand(command, $"Sends the advertisement '{command}' to the user using the command.", (player, commandInfo) =>
                {
                    if (player == null) return;

                    MessageColorFormatter formatter = new MessageColorFormatter();
                    string formattedPrefix = formatter.FormatMessage(Config.ChatPrefix);
                    string formattedMessage = formatter.FormatMessage(ad.Message, player.PlayerName, formattedPrefix);

                    player.PrintToChat($"{formattedMessage}");

                    string soundToPlay = ad.PlaySoundName ?? Config.GlobalPlaySound ?? string.Empty;
                    if (!ad.DisableSound && !string.IsNullOrWhiteSpace(soundToPlay))
                    {
                        player.ExecuteClientCommand($"play {soundToPlay}");
                    }
                });
            }
        }

    }

    public required BaseConfigs Config { get; set; }

    public void OnConfigParsed(BaseConfigs config)
    {
        ValidateConfig(config);
        Config = config;

        foreach (var ad in Config.Ads)
        {
            if (!lastAdTimes.ContainsKey(ad))
            {
                lastAdTimes[ad] = DateTime.MinValue;
            }
        }
    }

    private void ValidateConfig(BaseConfigs config)
    {
        foreach (var ad in config.Ads)
        {
            if (ad.Interval > 3600)
            {
                ad.Interval = 3600;
            }

            if (ad.Interval < 10)
            {
                ad.Interval = 10;
            }

            if (ad.triggerAd != null)
            {
                ad.triggerAd = ad.triggerAd.Distinct().ToList();
            }
        }

        if (config.ChatPrefix.Length > 80)
        {
            config.ChatPrefix = "[AutomaticAds]";
        }

        if (string.IsNullOrWhiteSpace(config.GlobalPlaySound))
        {
            config.GlobalPlaySound = "";
        }
    }

    private void OnMapStart(string mapName)
    {
        SendMessages();
    }

    public void SendMessages()
    {
        if (Config.SendAdsInOrder)
        {
            Config.Ads = Config.Ads.Where(ad => !ad.Disableinterval).ToList();
            ScheduleNextAd();
        }
        else
        {
            foreach (var ad in Config.Ads.Where(ad => !ad.Disableinterval))
            {
                var timer = AddTimer(1.00f, () =>
                {
                    if (CanSendAd(ad))
                    {
                        SendAdToPlayers(ad);
                        lastAdTimes[ad] = DateTime.Now;
                    }
                }, TimerFlags.REPEAT);

                timers.Add(timer);
            }
        }
    }

    private void ScheduleNextAd()
    {
        if (Config.Ads.Count == 0) return;

        var currentAd = Config.Ads[currentAdIndex];
        float interval = currentAd.Interval;

        adTimer?.Kill();
        adTimer = AddTimer(interval, () =>
        {
            if (CanSendAd(currentAd))
            {
                SendAdToPlayers(currentAd);
                lastAdTimes[currentAd] = DateTime.Now;
            }

            currentAdIndex = (currentAdIndex + 1) % Config.Ads.Count;
            ScheduleNextAd();
        });
    }

    private bool CanSendAd(BaseConfigs.AdConfig ad)
    {
        if (ad.Disableinterval)
        {
            return false;
        }

        if (!lastAdTimes.ContainsKey(ad))
        {
            lastAdTimes[ad] = DateTime.MinValue;
        }

        if (lastAdTimes[ad] == DateTime.MinValue)
        {
            return true;
        }

        string currentMap = _currentMap;
        // if (ad.Map != "all" && ad.Map != currentMap)
        if (ad.Map != "all")
        {
            if (!currentMap.StartsWith(ad.Map.Replace("*", "")))
            {
                return false;
            }
        }

        bool isWarmup = _gGameRulesProxy != null && _gGameRulesProxy.WarmupPeriod;
        if ((ad.OnlyInWarmup && !isWarmup) || (!ad.OnlyInWarmup && isWarmup))
        {
            return false;
        }

        var secondsSinceLastMessage = (int)(DateTime.Now - lastAdTimes[ad]).TotalSeconds;

        return secondsSinceLastMessage >= ad.Interval;
    }

    private void SendAdToPlayers(BaseConfigs.AdConfig ad)
    {
        var players = Utilities.GetPlayers();

        if (players == null || players.Count == 0)
        {
            return;
        }

        MessageColorFormatter formatter = new MessageColorFormatter();

        foreach (var player in players.Where(p => p != null && p.IsValid && p.Connected == PlayerConnectedState.PlayerConnected && !p.IsHLTV))
        {
            bool canView = string.IsNullOrWhiteSpace(ad.ViewFlag) || ad.ViewFlag == "all" || AdminManager.PlayerHasPermissions(player, ad.ViewFlag);
            bool isExcluded = !string.IsNullOrWhiteSpace(ad.ExcludeFlag) && AdminManager.PlayerHasPermissions(player, ad.ExcludeFlag);

            if (canView && !isExcluded)
            {
                string formattedPrefix = formatter.FormatMessage(Config.ChatPrefix);
                string formattedMessage = formatter.FormatMessage(ad.Message, player.PlayerName, formattedPrefix);

                player.PrintToChat($"{formattedMessage}");

                string soundToPlay = ad.PlaySoundName ?? Config.GlobalPlaySound ?? string.Empty;
                if (!ad.DisableSound && !string.IsNullOrWhiteSpace(soundToPlay))
                {
                    player.ExecuteClientCommand($"play {soundToPlay}");
                }
            }
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerFullConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player || player == null || !player.IsValid || player.IsBot || player.Connected != PlayerConnectedState.PlayerConnected)
            return HookResult.Continue;

        if (Config.EnableWelcomeMessage && !player.IsBot)
        {
            foreach (var welcome in Config.Welcome)
            {
                if (string.IsNullOrWhiteSpace(welcome.ViewFlag))
                {
                    welcome.ViewFlag = "all";
                }

                if (string.IsNullOrWhiteSpace(welcome.ExcludeFlag))
                {
                    welcome.ExcludeFlag = "";
                }

                bool canView = string.IsNullOrWhiteSpace(welcome.ViewFlag) || welcome.ViewFlag == "all" || AdminManager.PlayerHasPermissions(player, welcome.ViewFlag);
                bool isExcluded = !string.IsNullOrWhiteSpace(welcome.ExcludeFlag) && AdminManager.PlayerHasPermissions(player, welcome.ExcludeFlag);

                if (canView && !isExcluded)
                {
                    AddTimer(Config.WelcomeDelay, () =>
                    {
                        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)
                            return;

                        MessageColorFormatter formatter = new MessageColorFormatter();
                        string prefix = formatter.FormatMessage(Config.ChatPrefix);
                        string welcomeMessage = formatter.FormatMessage(welcome.WelcomeMessage, player.PlayerName);

                        player.PrintToChat($"{prefix} {welcomeMessage}");

                        if (!welcome.DisableSound && !string.IsNullOrWhiteSpace(Config.GlobalPlaySound))
                        {
                            player.ExecuteClientCommand($"play {Config.GlobalPlaySound}");
                        }
                    });
                }
            }
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnectPre(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (!Config.EnableJoinLeaveMessages || @event == null) return HookResult.Continue;

        info.DontBroadcast = true;

        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;
        var LeftPlayer = player.PlayerName;
        var SteamID64 = player.SteamID.ToString();
        var ipAddress = player.IpAddress?.Split(':')[0];

        if (Config.EnableJoinLeaveMessages && Config.JoinLeave != null && Config.JoinLeave.Any())
        {
            var leaveConfig = Config.JoinLeave.FirstOrDefault();
            if (leaveConfig != null)
            {
                MessageColorFormatter formatter = new MessageColorFormatter();
                string leaveMessage = formatter.FormatMessage(leaveConfig.LeaveMessage, LeftPlayer);

                if (leaveMessage.Contains("{id64}"))
                {
                    leaveMessage = leaveMessage.Replace("{id64}", SteamID64);
                }

                if (leaveMessage.Contains("{country}"))
                {
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        var query = new Query();
                        var countryName = query.GetCountryAsync(ipAddress).Result;
                        leaveMessage = leaveMessage.Replace("{country}", countryName ?? Localizer["Unknown"]);
                    }
                    else
                    {
                        leaveMessage = leaveMessage.Replace("{country}", Localizer["Unknown"]);
                    }
                }

                Server.PrintToChatAll(leaveMessage);
            }
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    private void OnClientPutInServer(int playerSlot)
    {
        var player = Utilities.GetPlayerFromSlot(playerSlot);
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return;

        var JoinPlayer = player.PlayerName;
        var SteamID64 = player.SteamID.ToString();
        var ipAddress = player.IpAddress?.Split(':')[0];

        if (Config.EnableJoinLeaveMessages && Config.JoinLeave != null && Config.JoinLeave.Any())
        {
            var joinConfig = Config.JoinLeave.FirstOrDefault();
            if (joinConfig != null)
            {
                MessageColorFormatter formatter = new MessageColorFormatter();
                string joinMessage = formatter.FormatMessage(joinConfig.JoinMessage, JoinPlayer);

                if (joinMessage.Contains("{id64}"))
                {
                    joinMessage = joinMessage.Replace("{id64}", SteamID64);
                }

                if (joinMessage.Contains("{country}"))
                {
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        var query = new Query();
                        var countryName = query.GetCountryAsync(ipAddress).Result;
                        joinMessage = joinMessage.Replace("{country}", countryName ?? Localizer["Unknown"]);
                    }
                    else
                    {
                        joinMessage = joinMessage.Replace("{country}", Localizer["Unknown"]);
                    }
                }

                Server.PrintToChatAll(joinMessage);
            }
        }
    }

    public override void Unload(bool hotReload)
    {
        adTimer?.Kill();
        foreach (var timer in timers)
        {
            timer.Kill();
        }
        timers.Clear();
    }
}
