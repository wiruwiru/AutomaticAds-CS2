namespace AutomaticAds.Utils;

public static class Constants
{
    public const int MaxInterval = 3600;
    public const int MinInterval = 10;
    public const int MaxPrefixLength = 80;
    public const string DefaultPrefix = "[AutomaticAds]";
    public const string DefaultGlobalSound = "ui/panorama/popup_reveal_01";
    public const string AllMapsKeyword = "all";
    public const string AllPlayersFlag = "all";
    public const string RootPermission = "@css/root";
    public const string VipPermission = "@css/vip";

    public const float MaxPositionX = 5.0f;
    public const float MinPositionX = -5.0f;
    public const float MaxPositionY = 3.0f;
    public const float MinPositionY = -3.0f;
    public const float DefaultPositionX = -1.5f;
    public const float DefaultPositionY = 1f;

    public static class ErrorMessages
    {
        public const string CountryCodeError = "CC Error";
        public const string GenericError = "Error";
        public const string Unknown = "Unknown";
    }

    public static class ApiUrls
    {
        public const string CountryApiBase = "https://api.country.is/";
    }
}