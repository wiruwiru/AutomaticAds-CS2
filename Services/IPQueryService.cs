using Newtonsoft.Json;
using AutomaticAds.Utils;
using AutomaticAds.Models;

namespace AutomaticAds.Services;

public interface IIPQueryService
{
    Task<string> GetCountryCodeAsync(string ipAddress);
    Task<string> GetCountryNameAsync(string ipAddress);
}

public class IPQueryService : IIPQueryService
{
    private static readonly HttpClient _httpClient = new();

    public async Task<string> GetCountryCodeAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return Constants.ErrorMessages.CountryCodeError;

        try
        {
            string requestUri = $"{Constants.ApiUrls.CountryApiBase}{ipAddress}";
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LogError($"Error getting country code. Status code: {response.StatusCode}");
                return Constants.ErrorMessages.CountryCodeError;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var countryResponse = JsonConvert.DeserializeObject<CountryApiResponse>(jsonResponse);

            if (countryResponse?.Country == null)
            {
                LogError("Country field is null in API response");
                return Constants.ErrorMessages.CountryCodeError;
            }

            return countryResponse.Country;
        }
        catch (HttpRequestException ex)
        {
            LogError($"HttpRequestException in GetCountryCodeAsync: {ex.Message}");
            return Constants.ErrorMessages.CountryCodeError;
        }
        catch (JsonException ex)
        {
            LogError($"JsonException in GetCountryCodeAsync: {ex.Message}");
            return Constants.ErrorMessages.CountryCodeError;
        }
        catch (Exception ex)
        {
            LogError($"Exception in GetCountryCodeAsync: {ex.Message}");
            return Constants.ErrorMessages.CountryCodeError;
        }
    }

    public async Task<string> GetCountryNameAsync(string ipAddress)
    {
        string countryCode = await GetCountryCodeAsync(ipAddress);

        if (countryCode == Constants.ErrorMessages.CountryCodeError)
            return Constants.ErrorMessages.Unknown;

        return CountryMapping.GetCountryName(countryCode);
    }

    private static void LogError(string message)
    {
        Console.WriteLine($"[AutomaticAds] {message}");
    }
}