using Azure.ResourceManager;
using Azure.ResourceManager.Resources;


namespace CertificateInventory.Services
{
    public interface IAzureManagementService
    {
        public SubscriptionCollection GetSubcriptions(string authenticationToken);
    }
}
