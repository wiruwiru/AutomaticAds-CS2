using Newtonsoft.Json;
using AutomaticAds.Utils;

namespace AutomaticAds.Services;

public interface IIPQueryService
{
    Task<string> GetCountryAsync(string ipAddress);
    Task<string> IPQueryAsync(string ipAddress, string endpoint);
}

public class IPQueryService : IIPQueryService
{
    private static readonly HttpClient _httpClient = new();

    public async Task<string> GetCountryAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return Constants.ErrorMessages.CountryCodeError;

        try
        {
            string requestUri = $"{Constants.ApiUrls.IpApiBase}{ipAddress}";
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LogError($"Error getting country code. Status code: {response.StatusCode}");
                return Constants.ErrorMessages.CountryCodeError;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            return data?.country?.ToString() ?? Constants.ErrorMessages.CountryCodeError;
        }
        catch (HttpRequestException ex)
        {
            LogError($"HttpRequestException in GetCountryAsync: {ex.Message}");
            return Constants.ErrorMessages.CountryCodeError;
        }
        catch (Exception ex)
        {
            LogError($"Exception in GetCountryAsync: {ex.Message}");
            return Constants.ErrorMessages.CountryCodeError;
        }
    }

    public async Task<string> IPQueryAsync(string ipAddress, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(endpoint))
            return Constants.ErrorMessages.GenericError;

        try
        {
            string apiUrl = $"{Constants.ApiUrls.IpApiCoBase}{ipAddress}/{endpoint}/";
            string response = await _httpClient.GetStringAsync(apiUrl).ConfigureAwait(false);
            return response?.Trim() ?? Constants.ErrorMessages.GenericError;
        }
        catch (HttpRequestException ex)
        {
            LogError($"HttpRequestException in IPQueryAsync: {ex.Message}");
            return Constants.ErrorMessages.GenericError;
        }
        catch (Exception ex)
        {
            LogError($"Exception in IPQueryAsync: {ex.Message}");
            return Constants.ErrorMessages.GenericError;
        }
    }

    private static void LogError(string message)
    {
        Console.WriteLine($"[AutomaticAds] {message}");
    }
}