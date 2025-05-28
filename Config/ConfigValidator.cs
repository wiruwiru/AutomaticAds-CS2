using AutomaticAds.Utils;

namespace AutomaticAds.Config;

public static class ConfigValidator
{
    public static void ValidateConfig(BaseConfigs config)
    {
        ValidateAds(config.Ads);
        ValidateChatPrefix(config);
        ValidateGlobalPlaySound(config);
    }

    private static void ValidateAds(List<BaseConfigs.AdConfig> ads)
    {
        foreach (var ad in ads)
        {
            ValidateAdInterval(ad);
            ValidateTriggerAd(ad);
        }
    }

    private static void ValidateAdInterval(BaseConfigs.AdConfig ad)
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

    private static void ValidateTriggerAd(BaseConfigs.AdConfig ad)
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
}