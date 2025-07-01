using AutomaticAds.Utils;
using AutomaticAds.Config.Models;

namespace AutomaticAds.Config;

public static class ConfigValidator
{
    public static void ValidateConfig(BaseConfigs config)
    {
        ValidateGlobalInterval(config);
        ValidateGlobalPositions(config);
        ValidateAds(config.Ads, config.GlobalInterval, config.GlobalPositionX, config.GlobalPositionY);
        ValidateChatPrefix(config);
        ValidateGlobalPlaySound(config);
        ValidateCenterHtmlDisplayTime(config);
        ValidateScreenDisplayTime(config);
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

    private static void ValidateGlobalPositions(BaseConfigs config)
    {
        if (config.GlobalPositionX > Constants.MaxPositionX)
        {
            config.GlobalPositionX = Constants.MaxPositionX;
        }
        if (config.GlobalPositionX < Constants.MinPositionX)
        {
            config.GlobalPositionX = Constants.MinPositionX;
        }

        if (config.GlobalPositionY > Constants.MaxPositionY)
        {
            config.GlobalPositionY = Constants.MaxPositionY;
        }
        if (config.GlobalPositionY < Constants.MinPositionY)
        {
            config.GlobalPositionY = Constants.MinPositionY;
        }
    }

    private static void ValidateAds(List<AdConfig> ads, float globalInterval, float globalPositionX, float globalPositionY)
    {
        foreach (var ad in ads)
        {
            ValidateAdInterval(ad, globalInterval);
            ValidateAdPositions(ad, globalPositionX, globalPositionY);
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

    private static void ValidateAdPositions(AdConfig ad, float globalPositionX, float globalPositionY)
    {
        float effectivePositionX = ad.GetEffectivePositionX(globalPositionX);
        if (effectivePositionX > Constants.MaxPositionX)
        {
            if (ad.HasCustomPositionX)
            {
                ad.PositionXRaw = Constants.MaxPositionX;
            }
        }
        if (effectivePositionX < Constants.MinPositionX)
        {
            if (ad.HasCustomPositionX)
            {
                ad.PositionXRaw = Constants.MinPositionX;
            }
        }
        ad.PositionX = ad.GetEffectivePositionX(globalPositionX);

        float effectivePositionY = ad.GetEffectivePositionY(globalPositionY);
        if (effectivePositionY > Constants.MaxPositionY)
        {
            if (ad.HasCustomPositionY)
            {
                ad.PositionYRaw = Constants.MaxPositionY;
            }
        }
        if (effectivePositionY < Constants.MinPositionY)
        {
            if (ad.HasCustomPositionY)
            {
                ad.PositionYRaw = Constants.MinPositionY;
            }
        }
        ad.PositionY = ad.GetEffectivePositionY(globalPositionY);
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

    private static void ValidateScreenDisplayTime(BaseConfigs config)
    {
        if (config.ScreenDisplayTime <= 0)
        {
            config.ScreenDisplayTime = 5.0f;
        }

        if (config.ScreenDisplayTime > 30.0f)
        {
            config.ScreenDisplayTime = 30.0f;
        }
    }
}