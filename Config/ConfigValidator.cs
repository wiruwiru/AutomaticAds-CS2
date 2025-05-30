using AutomaticAds.Utils;
using AutomaticAds.Config.Models;

namespace AutomaticAds.Config;

public static class ConfigValidator
{
    public static void ValidateConfig(BaseConfigs config)
    {
        ValidateGlobalInterval(config);
        ValidateAds(config.Ads, config.GlobalInterval);
        ValidateChatPrefix(config);
        ValidateGlobalPlaySound(config);
        ValidateCenterHtmlDisplayTime(config);
    }

    private static void ValidateGlobalInterval(BaseConfigs config)
    {
        if (config.GlobalInterval > Constants.MaxInterval)
        {
            config.GlobalInterval = Constants.MaxInterval;
        }

        if (config.GlobalInterval < Constants.MinInterval)
        {
            config.GlobalInterval = Constants.MinInterval;
        }
    }

    private static void ValidateAds(List<AdConfig> ads, float globalInterval)
    {
        foreach (var ad in ads)
        {
            ValidateAdInterval(ad, globalInterval);
            ValidateTriggerAd(ad);
        }
    }

    private static void ValidateAdInterval(AdConfig ad, float globalInterval)
    {
        float effectiveInterval = ad.GetEffectiveInterval(globalInterval);

        if (effectiveInterval > Constants.MaxInterval)
        {
            if (ad.HasCustomInterval)
            {
                ad.IntervalRaw = Constants.MaxInterval;
            }
            else
            {

            }
        }

        if (effectiveInterval < Constants.MinInterval)
        {
            if (ad.HasCustomInterval)
            {
                ad.IntervalRaw = Constants.MinInterval;
            }
        }

        ad.Interval = ad.GetEffectiveInterval(globalInterval);
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