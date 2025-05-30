using AutomaticAds.Utils;
using AutomaticAds.Config.Models;

namespace AutomaticAds.Config;

public static class ConfigValidator
{
    public static void ValidateConfig(BaseConfigs config)
    {
        ValidateAds(config.Ads);
        ValidateChatPrefix(config);
        ValidateGlobalPlaySound(config);
        ValidateCenterHtmlDisplayTime(config);
    }

    private static void ValidateAds(List<AdConfig> ads)
    {
        foreach (var ad in ads)
        {
            ValidateAdInterval(ad);
            ValidateTriggerAd(ad);
        }
    }

    private static void ValidateAdInterval(AdConfig ad)
    {
        if (ad.Interval > Constants.MaxInterval)
        {
            ad.Interval = Constants.MaxInterval;
        }

        if (ad.Interval < Constants.MinInterval)
        {
            ad.Interval = Constants.MinInterval;
        }
    }

    private static void ValidateTriggerAd(AdConfig ad)
    {
        if (ad.TriggerAd != null)
        {
            ad.TriggerAd = ad.TriggerAd.Distinct().ToList();
        }
    }

    private static void ValidateChatPrefix(BaseConfigs config)
    {
        if (config.ChatPrefix.Length > Constants.MaxPrefixLength)
        {
            config.ChatPrefix = Constants.DefaultPrefix;
        }
    }

    private static void ValidateGlobalPlaySound(BaseConfigs config)
    {
        if (string.IsNullOrWhiteSpace(config.GlobalPlaySound))
        {
            config.GlobalPlaySound = string.Empty;
        }
    }

    private static void ValidateCenterHtmlDisplayTime(BaseConfigs config)
    {
        if (config.centerHtmlDisplayTime <= 0)
        {
            config.centerHtmlDisplayTime = 5.0f;
        }
    }
}