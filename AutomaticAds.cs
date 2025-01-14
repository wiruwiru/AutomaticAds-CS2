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
    public override string ModuleVersion => "1.0.8";
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
        }

        if (config.ChatPrefix.Length > 80)
        {
            config.ChatPrefix = "[AutomaticAds]";
        }

        if (string.IsNullOrWhiteSpace(config.PlaySoundName))
        {
            config.PlaySoundName = "";
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
            ScheduleNextAd();
        }
        else
        {
            foreach (var ad in Config.Ads)
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
        if (!lastAdTimes.ContainsKey(ad))
        {
            lastAdTimes[ad] = DateTime.MinValue;
        }

        if (lastAdTimes[ad] == DateTime.MinValue)
        {
            return true;
        }

        string currentMap = _currentMap;
        if (ad.Map != "all" && ad.Map != currentMap)
        {
            return false;
        }

        bool isWarmup = _gGameRulesProxy != null && _gGameRulesProxy.WarmupPeriod;
        if ((ad.OnlyInWarmup && !isWarmup) || (!ad.OnlyInWarmup && isWarmup))
        {
            return false;
        }

        var secondsSinceLastMessage = (int)(DateTime.Now - lastAdTimes[ad]).TotalSeconds;

        bool canSend = secondsSinceLastMessage >= ad.Interval;
        return canSend;
    }

    private void SendAdToPlayers(BaseConfigs.AdConfig ad)
    {
        var players = Utilities.GetPlayers();

        if (players == null || players.Count == 0)
        {
            return;
        }

        MessageColorFormatter formatter = new MessageColorFormatter();
        string formattedPrefix = formatter.FormatMessage(Config.ChatPrefix);

        foreach (var player in players.Where(p => p != null && p.IsValid && p.Connected == PlayerConnectedState.PlayerConnected && !p.IsHLTV))
        {
            bool canView = string.IsNullOrWhiteSpace(ad.ViewFlag) || ad.ViewFlag == "all" || AdminManager.PlayerHasPermissions(player, ad.ViewFlag);
            bool isExcluded = !string.IsNullOrWhiteSpace(ad.ExcludeFlag) && AdminManager.PlayerHasPermissions(player, ad.ExcludeFlag);

            if (canView && !isExcluded)
            {
                string formattedMessage = formatter.FormatMessage(ad.Message, player.PlayerName);

                player.PrintToChat($"{formattedPrefix} {formattedMessage}");
                if (!ad.DisableSound && !string.IsNullOrWhiteSpace(Config.PlaySoundName))
                {
                    player.ExecuteClientCommand($"play {Config.PlaySoundName}");
                }
            }
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerFullConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player || player.IsBot)
            return HookResult.Continue;

        if (Config.EnableWelcomeMessage && player.IsValid && !player.IsBot)
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
                        MessageColorFormatter formatter = new MessageColorFormatter();
                        string prefix = formatter.FormatMessage(Config.ChatPrefix);
                        string welcomeMessage = formatter.FormatMessage(welcome.WelcomeMessage, player.PlayerName);

                        player.PrintToChat($"{prefix} {welcomeMessage}");

                        if (!welcome.DisableSound && !string.IsNullOrWhiteSpace(Config.PlaySoundName))
                        {
                            player.ExecuteClientCommand($"play {Config.PlaySoundName}");
                        }
                    });
                }
            }
        }

        return HookResult.Continue;
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
