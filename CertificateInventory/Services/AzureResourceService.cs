
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CertificateInventory.Services
{
    public class AzureResourceService:IAzureResourceService
    {
        private readonly ILogger<AzureResourceService> _logger;

        public AzureResourceService(ILogger<AzureResourceService> logger)
        {
            _logger = logger;
        }

        public async Task<T> GetResource<T>(string resourceUrl, string authenticationToken)
        {
            try
            {
                _logger.LogInformation($"AzureResourceService.GetResource - executing on resource URL: {resourceUrl}");

                using (HttpClient azureResourceClient = new HttpClient())
                {
                    azureResourceClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);

                    using (var request = new HttpRequestMessage(HttpMethod.Get, resourceUrl))
                    {
                        _logger.LogInformation("29");
                        using (var response = await azureResourceClient.SendAsync(request))
                        {
                            _logger.LogInformation("31");
                            if (response.IsSuccessStatusCode == false)
                            {
                                throw new HttpException($"ERROR: Failed to retrieve resource from {resourceUrl}. Invocation returned status code: {response.StatusCode}");
                            }
                            _logger.LogInformation("35");

                            return await response.Content.ReadAsAsync<T>();

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An exception occurred: {Message}", ex.Message);
                throw;
            }

        }
    }
}
