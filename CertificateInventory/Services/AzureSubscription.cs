using CertificateInventory.Repositories.Models;
using CertificateInventory.Repositories.Models.CloudServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateInventory.Services
{
    public class AzureSubscription : IAzureSubscription
    {
        private readonly IAzureResourceService _azureResourceService;
        private readonly ILogger<AzureSubscription> _logger;

        private readonly string _resourceUrl = "https://management.azure.com/subscriptions/{0}/resources?$filter=resourceType eq 'Microsoft.Compute/cloudServices'&api-version=2021-04-01";

        public AzureSubscription(ILogger<AzureSubscription> logger, IAzureResourceService azureResourceService)
        {
            _azureResourceService = azureResourceService;
            _logger = logger;
        }
        public async Task<List<CloudService>> GetCloudServices(string subscriptionId, string authenticationToken)
        {
            string resourceUrl =  string.Format(_resourceUrl, subscriptionId);

            _logger.LogInformation($"AzureSubscription.GetCloudServices - executing on subscription ID: {subscriptionId}");

            return await _azureResourceService.GetResource<List<CloudService>>(resourceUrl, authenticationToken);
        }

        public Task<List<KeyVault>> GetKeyVaults()
        {
            throw new NotImplementedException();
        }
    }
}
