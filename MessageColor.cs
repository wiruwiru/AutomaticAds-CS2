using CounterStrikeSharp.API.Modules.Utils;

namespace AutomaticAds;

public class MessageColorFormatter
{
    public string FormatMessage(string message)
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

        message = message.Replace("\n", "\u2029");
        message = System.Text.RegularExpressions.Regex.Replace(message, @"\{(.*?)\}", string.Empty);

        return message;
    }
}