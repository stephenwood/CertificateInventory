using CertificateInventory.Repositories.Models;

namespace CertificateInventory.Services
{
    public interface IAzureKeyVaultService
    {
        Task<KeyVaultCollection> GetKeyVaults(string subscriptionId, string authenticationToken);
        Task<KeyVault> GetKeyVault(string subscriptionId, string resourceGroupName, string vaultName, string authenticationToken);
        Task<IEnumerable<SecretVersion>> GetSecretVersions(string keyVaultName, string secretName, string authenticationToken);
        Task<IEnumerable<SecretMetadata>> GetSecrets(string keyVaultName, string authenticationToken);
        Task<string> GetSecret(string secretName, string authenticationToken);
        IEnumerable<SecretMetadata> FilterSecrets(IEnumerable<SecretMetadata> secrets, string azureKeyVaultToken);
    }
}
