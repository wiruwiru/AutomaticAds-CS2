using System.Net;
using MaxMind.Db;
using MaxMind.GeoIP2;
using Microsoft.Extensions.Logging;

using AutomaticAds.Utils;

namespace AutomaticAds.Services;

public interface IIPQueryService
{
    string GetCountryCode(string ipAddress);
    string GetCountryName(string ipAddress);
}

public class IPQueryService : IIPQueryService
{
    private readonly DatabaseReader? _reader;
    private readonly ILogger _logger;

    public IPQueryService(string moduleDirectory, ILogger logger)
    {
        _logger = logger;
        string dbPath = Path.Combine(moduleDirectory, "GeoLite2-Country.mmdb");
        _logger.LogInformation("Looking for GeoLite2-Country.mmdb at: {Path}", dbPath);

        if (!File.Exists(dbPath))
        {
            _logger.LogError("GeoLite2-Country.mmdb not found at: {Path}", dbPath);
            return;
        }

        _logger.LogInformation("Found mmdb file, size: {Size} bytes", new FileInfo(dbPath).Length);

        try
        {
            _reader = new DatabaseReader(dbPath, FileAccessMode.Memory);
            _logger.LogInformation("GeoLite2 database loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load GeoLite2 database");
        }
    }

    public string GetCountryCode(string ipAddress)
    {
        if (_reader == null || string.IsNullOrWhiteSpace(ipAddress))
        {
            _logger.LogInformation("GetCountryCode: reader={ReaderAvailable}, ip='{Ip}'", _reader != null, ipAddress);
            return Constants.ErrorMessages.CountryCodeError;
        }

        try
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
            {
                _logger.LogInformation("GetCountryCode: Invalid IP format '{Ip}'", ipAddress);
                return Constants.ErrorMessages.CountryCodeError;
            }

            if (_reader.TryCountry(ip, out var response) && response != null)
            {
                var isoCode = response.Country.IsoCode;
                _logger.LogInformation("GetCountryCode: IP={Ip}, IsoCode={IsoCode}", ipAddress, isoCode);
                return isoCode ?? Constants.ErrorMessages.CountryCodeError;
            }

            _logger.LogInformation("GetCountryCode: TryCountry returned false for IP={Ip}", ipAddress);
            return Constants.ErrorMessages.CountryCodeError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCountryCode exception for IP={Ip}", ipAddress);
            return Constants.ErrorMessages.CountryCodeError;
        }
    }

    public string GetCountryName(string ipAddress)
    {
        string countryCode = GetCountryCode(ipAddress);

        if (countryCode == Constants.ErrorMessages.CountryCodeError)
            return Constants.ErrorMessages.Unknown;

        return CountryMapping.GetCountryName(countryCode);
    }
}