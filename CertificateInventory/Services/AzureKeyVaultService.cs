using CertificateInventory.Repositories.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Web;

namespace CertificateInventory.Services
{
    public class AzureKeyVaultService : IAzureKeyVaultService
    {
        private readonly ILogger _logger;
        private readonly IAzureResourceService _azureResourceService;

        private string _secretUrl = "https://{0}.vault.azure.net/secrets/{1}?api-version=7.0";
        private string _secretsUrl = "https://{0}.vault.azure.net/secrets?api-version=7.0";
        private string _secretVersionsUrl = "https://{0}.vault.azure.net/secrets/{1}/versions?api-version=7.3";
        private string _keyVaultsUrl = "https://management.azure.com/subscriptions/{0}/resources?$filter=resourceType eq 'Microsoft.KeyVault/vaults'&api-version=2015-11-01";
        private string _keyVaultUrl = "https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.KeyVault/vaults/{vaultName}?api-version=2022-07-01";
        
        public AzureKeyVaultService(
            ILogger<AzureKeyVaultService> logger,
            IAzureResourceService azureResourceService)
        {
            _logger = logger;
            _azureResourceService = azureResourceService;
        }

        /// <summary>
        /// Retrieves a list of all the key vaults in the specified subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID.</param>
        /// <param name="authenticationToken">The authentication token used to authenticate against the end-point.</param>
        /// <returns></returns>
        public async Task<KeyVaultCollection> GetKeyVaults(string subscriptionId, string authenticationToken)
        {
            _logger.LogInformation($"AzureKeyVaultService: GetKeyVaults executing on subscription ID: {subscriptionId}.");

            string keyVaultsUrl = string.Format(_keyVaultsUrl, subscriptionId);

            try
            {
                KeyVaultCollection keyVaultCollection = await _azureResourceService.GetResource<KeyVaultCollection>(string.Format(_keyVaultsUrl, subscriptionId), authenticationToken);
                return keyVaultCollection;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"ERROR: Failed to retrieve key vaults from {keyVaultsUrl}. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the specified key vault.
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="vaultName"></param>
        /// <param name="authenticationToken"></param>
        /// <returns></returns>
        public async Task<KeyVault> GetKeyVault(string subscriptionId, string resourceGroupName, string vaultName, string authenticationToken)
        {
            _logger.LogInformation($"AzureKeyVaultService: GetKeyVault executing on subscription ID: {subscriptionId}, resource group {resourceGroupName}, and vault {vaultName}.");

            string keyVaultUrl = string.Format(_keyVaultUrl, subscriptionId, resourceGroupName, vaultName);

            KeyVault keyVault = await _azureResourceService.GetResource<KeyVault>(keyVaultUrl, authenticationToken);
            return keyVault;
        }

        public async Task<IEnumerable<SecretMetadata>> GetSecrets(string keyVault, string authenticationToken)
        {
            _logger.LogInformation($"AzureKeyVaultService: GetSecrets executing on key vault: {keyVault}.");

            string secretsUrl = String.Format(_secretsUrl, keyVault.ToLower());

            List<SecretMetadata> secrets = await _azureResourceService.GetResource<List<SecretMetadata>>(secretsUrl, authenticationToken);
            return secrets;
        }

        public async Task<string> GetSecret(string vault, string secretName, string authenticationToken)
        {
            vault = vault.ToLower();
            _secretUrl = string.Format(_secretUrl, vault, secretName);

            return await GetSecret(_secretUrl, authenticationToken);

        }
        public async Task<string> GetSecret(string secretUri, string authenticationToken)
        {
            _logger.LogInformation($"AzureKeyVaultService: GetSeccret executing: {secretUri}.");

            try
            {
                using (var azureKeyVaultClient = new HttpClient())
                {
                    azureKeyVaultClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);

                    using (var request = new HttpRequestMessage(HttpMethod.Get, secretUri))
                    {
                        using (var response = await azureKeyVaultClient.SendAsync(request))
                        {

                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogInformation($"ERROR: Failed to retrieve secret from {secretUri}. HTTP Error Code: {response.StatusCode}");
                            }

                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"ERROR: Failed to retrieve secret from {secretUri} with authentication token {authenticationToken}. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of all the versions of hte specified secret.
        /// </summary>
        /// <param name="keyVaultName">The name of the key vault that contains the secret</param>
        /// <param name="secretName">The name of the secret.</param>
        /// <param name="authenticationToken">The token with which to authenticate against the key vault.</param>
        /// <returns>A collection of the secret versions. The caller will typically retrieve the most recent secret in the list.</returns>
        public async Task<IEnumerable<SecretVersion>> GetSecretVersions(string keyVaultName, string secretName, string authenticationToken)
        {
            _logger.LogInformation($"Getting secret versions for {secretName} in {keyVaultName}.");

            using (var azureKeyVaultClient = new HttpClient())
            {
                azureKeyVaultClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticationToken}");

                var requestUri = string.Format(this._secretVersionsUrl, keyVaultName.ToLower(), secretName);

                _logger.LogInformation($"Request URI: {requestUri}");

                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using (var response = await azureKeyVaultClient.SendAsync(requestMessage))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpException($"ERROR: Failed to retrieve secret versions for {secretName} in {keyVaultName}. {response.ReasonPhrase}");
                        }

                        var responseContent = await response.Content.ReadAsAsync<dynamic>();

                        List<SecretVersion> secretVersions = responseContent.value.ToObject<List<SecretVersion>>();

                        return secretVersions;
                    }
                }
            }
        }

        public IEnumerable<SecretMetadata> FilterSecrets(IEnumerable<SecretMetadata> secrets, string azureKeyVaultToken)
        {
            throw new NotImplementedException();
        }
    }
}
