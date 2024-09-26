using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateInventory.Services
{
    public class AzureManagementService : IAzureManagementService
    {
        private readonly ILogger _logger;
        public AzureManagementService(ILogger<AzureManagementService> logger) 
        {
            _logger = logger;
        }

        public SubscriptionCollection GetSubcriptions(string authenticationToken)
        {
            _logger.LogInformation("AzureManagementService.GetSubcriptions - retrieving all subscriptions for this tenant...");

            ArmClient armClient = new ArmClient(new DefaultAzureCredential());
            SubscriptionCollection subscriptionCollection = armClient.GetSubscriptions();
            return subscriptionCollection;
        }
    }
}
