using Newtonsoft.Json;

namespace AutomaticAds
{
    internal class Query : IQuery
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<string> GetCountryAsync(string ipAddress)
        {
            try
            {
                string requestUri = $"http://ip-api.com/json/{ipAddress}";
                HttpResponseMessage response = await client.GetAsync(requestUri).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error getting country code. Status code: {response.StatusCode}");
                    return "CC Error";
                }

                string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                return data?.country?.ToString() ?? "CC Error";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException in GetCountryAsync: {ex.Message}");
                return "CC Error";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetCountryAsync: {ex.Message}");
                return "CC Error";
            }
        }

        public async Task<string> IPQueryAsync(string ipAddress, string endpoint)
        {
            try
            {
                string apiUrl = $"https://ipapi.co/{ipAddress}/{endpoint}/";
                string response = await client.GetStringAsync(apiUrl).ConfigureAwait(false);
                return response?.Trim() ?? "Error";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException in IPQueryAsync: {ex.Message}");
                return "Error";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in IPQueryAsync: {ex.Message}");
                return "Error";
            }
        }
    }

    internal interface IQuery
    {
        Task<string> GetCountryAsync(string ipAddress);
        Task<string> IPQueryAsync(string ipAddress, string endpoint);
    }
}
