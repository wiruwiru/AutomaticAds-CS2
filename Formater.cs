using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;

namespace AutomaticAds;

public class MessageColorFormatter
{
    public string FormatMessage(string message, string playerName = "")
    {
        var validColors = new Dictionary<string, string>
        {
            { "{GREEN}", ChatColors.Green.ToString() },
            { "{RED}", ChatColors.Red.ToString() },
            { "{YELLOW}", ChatColors.Yellow.ToString() },
            { "{BLUE}", ChatColors.Blue.ToString() },
            { "{PURPLE}", ChatColors.Purple.ToString() },
            { "{ORANGE}", ChatColors.Orange.ToString() },
            { "{WHITE}", ChatColors.White.ToString() },
            { "{NORMAL}", ChatColors.White.ToString() },
            { "{GREY}", ChatColors.Grey.ToString() },
            { "{LIGHT_RED}", ChatColors.LightRed.ToString() },
            { "{LIGHT_BLUE}", ChatColors.LightBlue.ToString() },
            { "{LIGHT_PURPLE}", ChatColors.LightPurple.ToString() },
            { "{LIGHT_YELLOW}", ChatColors.LightYellow.ToString() },
            { "{DARK_RED}", ChatColors.DarkRed.ToString() },
            { "{DARK_BLUE}", ChatColors.DarkBlue.ToString() },
            { "{BLUE_GREY}", ChatColors.BlueGrey.ToString() },
            { "{OLIVE}", ChatColors.Olive.ToString() },
            { "{LIME}", ChatColors.Lime.ToString() },
            { "{GOLD}", ChatColors.Gold.ToString() },
            { "{SILVER}", ChatColors.Silver.ToString() },
            { "{MAGENTA}", ChatColors.Magenta.ToString() },
        };

        foreach (var color in validColors)
        {
            message = message.Replace(color.Key, color.Value);
        }

        message = message.Replace("{playername}", playerName);
        message = message.Replace("\n", "\u2029");
        message = ReplaceServerVariables(message);

        return message;
    }

    private string ReplaceServerVariables(string message)
    {
        string ip = $"{ConVar.Find("ip")?.StringValue}:{ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString()}";
        string hostname = ConVar.Find("hostname")?.StringValue ?? "Unknown";
        string map = Server.MapName;
        string time = DateTime.Now.ToString("HH:mm");
        string date = DateTime.Now.ToString("yyyy-MM-dd");
        int players = Utilities.GetPlayers().Count(p => !p.IsBot && !p.IsHLTV);
        int maxPlayers = Server.MaxPlayers;

        var variables = new Dictionary<string, string>
        {
            { "{ip}", ip },
            { "{hostname}", hostname },
            { "{map}", map },
            { "{time}", time },
            { "{date}", date },
            { "{players}", players.ToString() },
            { "{maxplayers}", maxPlayers.ToString() }
        };

        foreach (var variable in variables)
        {
            message = message.Replace(variable.Key, variable.Value);
        }

        return message;
    }
}