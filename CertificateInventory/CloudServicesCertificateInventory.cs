using Azure.ResourceManager.Resources;
using CertificateInventory.Services;
using CertificateInventory.Repositories;
using CertificateInventory.Repositories.Models;
using CertificateInventory.Repositories.Models.CloudServices;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CertificateInventory
{
    public class CloudServicesCertificateInventory
    {
        private readonly ILogger _logger;
        private readonly IAzureAccessTokenService _azureAccessTokenService;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly IAzureManagementService _azureManagementService;
        private readonly IAzureCloudService _azureCloudService;
        private readonly IAzureCertificateService _azureCertificateService;
        private readonly IAzureSecretContainerService _azureSecretContainerService;
        private readonly ICertificateRepository _certificateRepository;

        public CloudServicesCertificateInventory(
            ILogger<CloudServicesCertificateInventory> logger,
            IAzureAccessTokenService azureAccessTokenService,
            IAzureKeyVaultService azureKeyVaultService,
            IAzureManagementService azureManagementService,
            IAzureCloudService azureCloudServiceService,
            IAzureCertificateService azureCertificateService,
            IAzureSecretContainerService azureSecretContainerService,
            ICertificateRepository certificateRepository)
        {
            _logger = logger;
            _azureAccessTokenService = azureAccessTokenService;
            _azureKeyVaultService = azureKeyVaultService;
            _azureManagementService = azureManagementService;
            _azureCloudService = azureCloudServiceService;
            _azureCertificateService = azureCertificateService;
            _azureSecretContainerService = azureSecretContainerService;
            _certificateRepository = certificateRepository;
        }

        [Function("Cloud-Services-Certificate-Inventory")]
        public async Task Run([TimerTrigger("0 0 23 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Cloud-Services-Certificate-Inventory started at: {DateTime.Now}");

            try
            {
                // Retrieve the Azure access tokens that we need to access the Azure Management API and Azure Key Vault

                _logger.LogInformation("Retrieving Azure management token...");

                string azureManagementToken = await _azureAccessTokenService.GetToken("https://management.azure.com/");

                _logger.LogInformation("Azure management token retrieved successfully!");
                _logger.LogInformation("Retreiving Azure Key Vault token...");

                string azureKeyVaultToken = await _azureAccessTokenService.GetToken("https://vault.azure.net");

                _logger.LogInformation("Azure Key Vault token retrieved successfully!");
                _logger.LogInformation("Retrieving all subscriptions for this tenant...");

                //// Retrieve all subscriptions for this tenant.

                SubscriptionCollection subscriptionCollection = _azureManagementService.GetSubcriptions(azureManagementToken);

                var subscriptionName = System.Environment.GetEnvironmentVariable("SubscriptionName", EnvironmentVariableTarget.Process) ?? throw new NullReferenceException("SubscriptionName environment variable not set.");
                var environmentName = System.Environment.GetEnvironmentVariable("EnvironmentName", EnvironmentVariableTarget.Process) ?? throw new NullReferenceException("EnvironmentName environment variable not set.");

                _logger.LogInformation($"Retrieving subscription {subscriptionName}, environment {environmentName}...");

                // So the idea in the code that follows is this. Every subscription may have one or more cloud services,
                // and each cloud service may have one or more secret containers, and every secret container may have one or 
                // more certificates. The rest of this function implements the looping logic required to get all those certificates
                // and write metadata about them to the Inventory database.

                var lensSubscription = subscriptionCollection.FirstOrDefault(x => x.Data.DisplayName == subscriptionName);
                string lensSubscriptionId = lensSubscription?.Data.SubscriptionId.Replace("/subscriptions/", "") ?? string.Empty;

                // Get all the Cloud Services for this subscription.

                _logger.LogInformation($"Retrieving cloud services for subscription ID {lensSubscriptionId}");

                CloudServiceCollection cloudServiceCollection = new CloudServiceCollection();

                try
                {
                    cloudServiceCollection = await _azureCloudService.GetCloudServices(lensSubscriptionId, azureManagementToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error retrieving cloud services for subscription ID {lensSubscriptionId}: {ex.Message}");
                }

                // Build a collection of certificates to add to the repository, and add them in a single operation.

                List<CertificateMetadata> certificateCollection = new List<CertificateMetadata>();
                List<CloudService> cloudServices = cloudServiceCollection?.CloudServices ?? throw new Exception("Cloud services collection is null.");

                foreach (CloudService cloudService in cloudServiceCollection.CloudServices)
                {
                    _logger.LogInformation($"Retrieving secret containers for cloud service {cloudService.Name}...");

                    string resourceGroupName = cloudService.Id != null ? cloudService.Id.Split("/resourceGroups/")[1].Split('/')[0] : string.Empty;
                    string cloudServiceName = cloudService.Name ?? string.Empty;

                    IEnumerable<SecretContainer> secretContainers = await _azureSecretContainerService.GetSecretContainers(lensSubscriptionId, resourceGroupName, cloudServiceName, azureManagementToken);

                    _logger.LogInformation($"Retrieved {secretContainers.Count()} secret containers for cloud service {cloudService.Name}!");

                    foreach (SecretContainer secretContainer in secretContainers)
                    {
                        _logger.LogInformation($"Retrieving possible certificates from secret container {secretContainer.sourceVault}...");

                        foreach (VaultCertificate vaultCertificate in secretContainer.vaultCertificates)
                        {
                            _logger.LogInformation($"Retrieving possible certificates from vault certificate  {vaultCertificate.certificateUrl}");

                            CertificateMetadata? certificate = await _azureSecretContainerService.GetCertificate(cloudService, vaultCertificate, azureKeyVaultToken);

                            if (certificate != null)
                            {
                                certificateCollection.Add(certificate);
                            }
                        }
                    }
                }

                _logger.LogInformation($"Adding {certificateCollection.Count} certificates to the Inventory database...");

                // NOTE TO SELF:  This is actually wrong. We need to do an upsert here.

                await _certificateRepository.CreateCertificates(certificateCollection);
            }
            catch (HttpException ex)
            {
                _logger.LogError(ex, $"ERROR: Http invocation failed with message: {ex.Message}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Cloud-Services-Certificate-Inventory: {ex.Message}");
            }
        }
    }
}
