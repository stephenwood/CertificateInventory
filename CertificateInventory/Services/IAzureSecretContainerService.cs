using CertificateInventory.Repositories.Models;
using CertificateInventory.Repositories.Models.CloudServices;

namespace CertificateInventory.Services
{
    public interface IAzureSecretContainerService
    {
        Task<List<SecretContainer>> GetSecretContainers(string subscriptionId, string resourceGroupName, string cloudServiceName, string authenticationToken);
        Task<CertificateMetadata?> GetCertificate(CloudService cloudService, VaultCertificate vaultCertificate, string authenticationToken);
    }
}
