using CertificateInventory.Repositories.Models;
using CertificateInventory.Repositories.Models.CloudServices;

namespace CertificateInventory.Services
{
    public interface IAzureSubscription
    {
        public Task<List<CloudService>> GetCloudServices(string subscriptionId, string authenticationToken);
        public Task<List<KeyVault>> GetKeyVaults();
    }
}
