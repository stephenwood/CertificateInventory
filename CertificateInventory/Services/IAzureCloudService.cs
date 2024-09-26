using CertificateInventory.Repositories.Models.CloudServices;

namespace CertificateInventory.Services
{
    public interface IAzureCloudService
    {
        public Task<CloudServiceCollection> GetCloudServices(string subscriptionId, string authenticationToken);
    }
}
