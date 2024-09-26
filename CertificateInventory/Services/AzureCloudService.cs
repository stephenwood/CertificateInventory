using Azure;
using CertificateInventory.Repositories.Models;
using CertificateInventory.Repositories.Models.CloudServices;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CertificateInventory.Services
{
    public class AzureCloudService:IAzureCloudService
    {
        private readonly ILogger<AzureCloudService> _logger;
        private readonly IAzureResourceService _azureResourceService;
        private readonly string _resourceUrl = "https://management.azure.com/subscriptions/{0}/resources?$filter=resourceType eq 'Microsoft.Compute/cloudServices'&api-version=2021-04-01";

        public AzureCloudService(
            ILogger<AzureCloudService> logger,
            IAzureResourceService azureResourceService) 
        {
            _logger = logger;
            _azureResourceService = azureResourceService;
        }
        public async Task<CloudServiceCollection> GetCloudServices(string subscriptionId, string authenticationToken)
        {
            _logger.LogInformation($"AzureCloudService.GetCloudServices - executing on subscription ID: {subscriptionId}");

            string resourceUrl = string.Format(_resourceUrl, subscriptionId);

            try
            {
                // We should not have to do this, and I'm frankly not sure why we do. You should be able to do something like this:
                // var responseContent = await _azureResourceService.GetResource<CloudServiceCollection>(resourceUrl, authenticationToken); 
                // But I have yet to get that to work.

                _logger.LogInformation($"Retrieving cloud services URL {resourceUrl}.");

                dynamic responseContent = await _azureResourceService.GetResource<dynamic>(resourceUrl, authenticationToken);
                List<CloudService> cloudServices = responseContent.value.ToObject<List<CloudService>>();

                CloudServiceCollection cloudServiceCollection = new CloudServiceCollection();
                cloudServiceCollection.CloudServices = cloudServices;

                return cloudServiceCollection;
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"An exception occurred: {ex.Message}");
                throw;
            }
        }
    }
}
