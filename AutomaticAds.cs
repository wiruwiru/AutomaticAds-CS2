using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Modules.Timers;
using static CounterStrikeSharp.API.Core.Listeners;

namespace AutomaticAds;

[MinimumApiVersion(247)]
public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("ChatPrefix")]
    public string ChatPrefix { get; set; } = " [{GREEN}AutomaticAds]";

    [JsonPropertyName("ChatMessage")]
    public string ChatMessage { get; set; } = "{RED}AutomaticAds is the best plugin!";

    [JsonPropertyName("AdminFlag")]
    public string AdminFlag { get; set; } = "css_root";

    [JsonPropertyName("ForceCommand")]
    public string ForceCommand { get; set; } = "css_announce";

    [JsonPropertyName("SendForceCommand")]
    public string SendForceCommand { get; set; } = "{RED}You have successfully forced the ad to be sent.";

    [JsonPropertyName("NoPermissions")]
    public string NoPermissions { get; set; } = "{RED}You do not have permissions to use this command.";

    [JsonPropertyName("ChatInterval")]
    public float ChatInterval { get; set; } = 600;

    [JsonPropertyName("PlaySoundName")]
    public string? PlaySoundName { get; set; } = "ui/panorama/popup_reveal_01";
}

public class AutomaticAdsBase : BasePlugin, IPluginConfig<BaseConfigs>
{
    public override string ModuleName => "AutomaticAds";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "luca.uy and iancg";
    public override string ModuleDescription => "I send automatic messages to the chat and play a sound alert for users to see the message.";

    private bool _isDebugEnabled = false; // ACTIVAR O DESACTIVAR LOS MENSAJES DE DEBUG/LOGS
    public CounterStrikeSharp.API.Modules.Timers.Timer? intervalMessages;
    public override void Load(bool hotReload)
    {
        AddCommand(Config.ForceCommand, "Send the advertisement in a forced way", (player, commandInfo) =>
        {
            if (player == null) return;

            Log($"AdminFlag configurada: {Config.AdminFlag}");

            var validador = new RequiresPermissions(@Config.AdminFlag);
            validador.Command = Config.ForceCommand;

            if (!validador.CanExecuteCommand(player))
            {
                string formattedNoPermissions = FormatMessage(Config.NoPermissions);
                Log($"Mensaje de no permisos formateado: {formattedNoPermissions}");
                player.PrintToChat($"{FormatMessage(Config.ChatPrefix)} {formattedNoPermissions}");
                return;
            }

            Log("Comando para forzar el envío del mensaje recibido.");
            SendMessageToAllPlayers($"{FormatMessage(Config.ChatPrefix)} {FormatMessage(Config.ChatMessage)}");
            Log("Mensaje forzado enviado a todos los jugadores.");

            string formattedSendForceCommand = FormatMessage(Config.SendForceCommand);
            commandInfo.ReplyToCommand($"{FormatMessage(Config.ChatPrefix)} {formattedSendForceCommand}");
        });
        
        RegisterListener<Listeners.OnMapEnd>(() => Unload(true));
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    public required BaseConfigs Config { get; set; }

    private DateTime _lastMessageTime = DateTime.Now;

    public void OnConfigParsed(BaseConfigs config)
    {
        Log("Configuración analizada.");
        ValidateConfig(config);
        Config = config;
        Log("Configuración validada.");
    }

    private void ValidateConfig(BaseConfigs config)
    {
        Log("Validando configuración...");

        if (config.ChatInterval > 3600)
        {
            config.ChatInterval = 3600;
            Log("ChatInterval ajustado a 3600 segundos (máximo permitido).");
        }

        if (config.ChatInterval < 10)
        {
            config.ChatInterval = 10;
            Log("ChatInterval ajustado a 10 segundos (mínimo permitido).");
        }

        if (config.ChatPrefix.Length > 80)
        {
            config.ChatPrefix = "[AutomaticAds]";
            Log("ChatPrefix ajustado a '[AutomaticAds]' (longitud máxima alcanzada).");
        }

        if (config.ChatMessage.Length > 400)
        {
            config.ChatMessage = "Your message is too large to send.";
            Log("ChatMessage ajustado: 'Your message es demasiado largo para enviar.'");
        }

        if (string.IsNullOrWhiteSpace(config.PlaySoundName))
        {
            config.PlaySoundName = "";
            Log("PlaySoundName ajustado a vacío (no se proporcionó sonido).");
        }
    }

    public void SendMessages()
    {
        intervalMessages = AddTimer(1.00f, () => {
            Log("Inicio de ronda detectado.");
            if (CanSendMessage())
            {
                SendMessageToAllPlayers($"{Config.ChatPrefix} {Config.ChatMessage}");
                _lastMessageTime = DateTime.Now;
                Log("Mensaje enviado a todos los jugadores.");
            }
            else
            {
                Log("No se puede enviar el mensaje en este momento.");
            }
        }, TimerFlags.REPEAT);
    }

    private bool CanSendMessage()
    {
        var secondsSinceLastMessage = (int)(DateTime.Now - _lastMessageTime).TotalSeconds;
        Log($"Han pasado {secondsSinceLastMessage} segundos desde el último mensaje.");
        return secondsSinceLastMessage >= Config.ChatInterval;
    }

    private void SendMessageToAllPlayers(string message)
    {
        Log("Enviando mensaje a todos los jugadores...");
        var players = Utilities.GetPlayers();

        if (players == null || players.Count == 0)
        {
            Log("No se encontraron jugadores.");
            return;
        }

        MessageColorFormatter formatter = new MessageColorFormatter();

        string formattedPrefix = formatter.FormatMessage(Config.ChatPrefix);
        string formattedMessage = formatter.FormatMessage(Config.ChatMessage);

        string finalMessage = $"{formattedPrefix} {formattedMessage}";

        Log($"Mensaje final a enviar: {finalMessage}");

        foreach (var player in players.Where(player => player != null && player.IsValid && player.Connected == PlayerConnectedState.PlayerConnected && !player.IsHLTV))
        {
            string playerName = player.PlayerName;

            player.PrintToChat(finalMessage);
            Log($"Mensaje enviado a {playerName}: {finalMessage}");

            if (!string.IsNullOrWhiteSpace(Config.PlaySoundName))
            {
                player.ExecuteClientCommand($"play {Config.PlaySoundName}");
                Log($"Sonido reproducido para {playerName}: {Config.PlaySoundName}");
            }
        }
    }

    private string FormatMessage(string message)
    {
        MessageColorFormatter formatter = new MessageColorFormatter();
        return formatter.FormatMessage(message);
    }

    private void Log(string message)
    {
        if (_isDebugEnabled)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }
    }

    public void SetDebugEnabled(bool isEnabled)
    {
        _isDebugEnabled = isEnabled;
    }
    private void OnMapStart(string mapName)
    {
        SendMessages();
    }
    public override void Unload(bool hotReload)
    {
        intervalMessages?.Kill();
    }
}