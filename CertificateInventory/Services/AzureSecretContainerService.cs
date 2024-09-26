using CertificateInventory.Repositories.Models;
using CertificateInventory.Repositories.Models.CloudServices;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace CertificateInventory.Services
{
    public class AzureSecretContainerService : IAzureSecretContainerService
    {
        private string _azureSecretContainerUrl = "https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Compute/cloudServices/{2}?api-version=2022-04-04";
        private readonly ILogger<AzureSecretContainerService> _logger;

        public AzureSecretContainerService(ILogger<AzureSecretContainerService> logger)
        {
            _logger = logger;
        }
        public async Task<CertificateMetadata?> GetCertificate(CloudService cloudService, VaultCertificate vaultCertificate, string authenticationToken)
        {

            List<CertificateMetadata> certificates = new List<CertificateMetadata>();

            try
            {
                string resourceGroupName = cloudService.Id != null ? cloudService.Id.Split("/resourceGroups/")[1].Split('/')[0] : throw new InvalidDataException("The Cloud Service ID is null.");

                _logger.LogInformation($"AzureSecretContainerService.GetCertificates: Getting certificates for cloud service {cloudService.Name} in resource group {resourceGroupName}.");

                var subscriptionName = System.Environment.GetEnvironmentVariable("SubscriptionName", EnvironmentVariableTarget.Process) ?? throw new NullReferenceException("SubscriptionName environment variable not set.");
                var environmentName = System.Environment.GetEnvironmentVariable("EnvironmentName", EnvironmentVariableTarget.Process) ?? throw new NullReferenceException("EnvironmentName environment variable not set.");

                _logger.LogInformation($"AzureSecretContainerService.GetCertificates: transforming certificate URL: {vaultCertificate.certificateUrl}.");

                string vaultCertificateUrl = vaultCertificate.certificateUrl.Replace("/secrets/", "/certificates/") + "?api-version=7.3";

                _logger.LogInformation($"AzureSecretContainerService.GetCertificates: Getting certificate from vault {vaultCertificateUrl}.");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);

                    using (var request = new HttpRequestMessage(HttpMethod.Get, vaultCertificateUrl))
                    {

                        using (var response = await client.SendAsync(request))
                        {
                            // If we fail to get the certificate, log the error and continue to the next certificate.
                            if (response.IsSuccessStatusCode == false)
                            {
                                _logger.LogInformation($"ERROR retrieving certificate for {vaultCertificateUrl}. Status code: {response.StatusCode}");
                            }

                            _logger.LogInformation($"AzureSecretContainerService.GetCertificates: HTTP status code is {response.StatusCode}.");

                            // Convert the raw certificate to a cert metadata object, then add a few attributes that are of interest
                            // to us, but that are not part of the raw certificate.

                            var potentialCertificate = await response.Content.ReadAsAsync<dynamic>();
                            var potentialCertificateString = potentialCertificate.cer.ToObject<string>();

                            CertificateMetadata? certificateMetadata = TryGetCertificate(potentialCertificateString, out bool succeeded);

                            if (succeeded && certificateMetadata != null)
                            {
                                _logger.LogInformation("Certificate retrieval succeeded!");

                                certificateMetadata.ResourceName = cloudService.Name;
                                certificateMetadata.ResourceEndpoint = $"{cloudService.Name}.{cloudService.Location}.cloudapp.azure.com";
                                certificateMetadata.Region = cloudService.Location;
                                certificateMetadata.LastDetectedInstalledUtcDateTime = DateTime.UtcNow;
                                certificateMetadata.LastInventoriedUtcDateTime = DateTime.UtcNow;
                                certificateMetadata.ScanType = "Cloud-Services-Certificate-Inventory";
                                certificateMetadata.ResourceType = "Azure Cloud Service";
                                certificateMetadata.ResourceEnvironment = environmentName;
                                certificateMetadata.ResourceAzureSubscription = subscriptionName;
                                certificateMetadata.ScanType = "Azure CloudService Get";

                                return certificateMetadata;
                            }
                            else
                            {
                                _logger.LogInformation("Certificate retrieval failed.");
                                return null;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"AzureSecretContainerService.GetCertificates: Error getting certificates for cloud service {cloudService.Name}. {ex.Message}");
                return null;
            }

        }

        /// <summary>
        /// Verifies whether a string blob is a certificate. It does this by attempting to convert the string to a certificate.
        /// </summary>
        /// <param name="possibleCertificate"></param>
        /// <param name="succeeded"></param>
        /// <returns></returns>
        protected CertificateMetadata? TryGetCertificate(string possibleCertificate, out bool succeeded)
        {
            try
            {
                _logger.LogInformation($"AzureSecretContainerService.TryGetCertificate: executing.");

                X509Certificate2 x509Certificate = new X509Certificate2(Convert.FromBase64String(possibleCertificate));
                CertificateMetadata certificateMetadata = GetCertificateMetatdata(x509Certificate, string.Empty);
                succeeded = true;
                return certificateMetadata;

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"AzureSecretContainerService.TryGetCertificate: Error converting string to certificate. This requires no remedial action. Execption is {ex.Message}");
                succeeded = false;
                return null;
            }
        }

        private CertificateMetadata GetCertificateMetatdata(
            X509Certificate2 x509Certificate,
            string certificateStore)
        {
            var certificateExtensions = new X509Extension[x509Certificate.Extensions.Count];
            x509Certificate.Extensions.CopyTo(certificateExtensions, 0);

            string basicConstraints = string.Empty;

            var basicConstraintsObject = certificateExtensions.FirstOrDefault(e => e.Oid?.Value == "Basic Constraints");

            if (basicConstraintsObject != null)
            {
                var asnBasicConstraintsData = new AsnEncodedData(basicConstraintsObject.Oid, basicConstraintsObject.RawData);
                basicConstraints = asnBasicConstraintsData.Format(true);

                if (basicConstraints.EndsWith("\r\n"))
                {
                    basicConstraints = basicConstraints.Substring(0, basicConstraints.LastIndexOf("\r\n"));
                    basicConstraints = basicConstraints.Replace("\r\n", ", ");
                }
            }

            string subjectAlternativeName = string.Empty;

            var sanObject = certificateExtensions.FirstOrDefault(x => x.Oid?.FriendlyName == "Subject Alternative Name");
            if (sanObject != null)
            {
                var asnSubjectAlternativeNameData = new AsnEncodedData(sanObject.Oid, sanObject.RawData);
                subjectAlternativeName = asnSubjectAlternativeNameData.Format(true);
                if (subjectAlternativeName.EndsWith("\r\n")) { subjectAlternativeName = subjectAlternativeName.Substring(0, subjectAlternativeName.LastIndexOf("\r\n")); }
                subjectAlternativeName = subjectAlternativeName.Replace("\r\n", ", ");
            }

            return new CertificateMetadata
            {
                CertificateStore = certificateStore,
                CertificateThumbprint = x509Certificate.Thumbprint,
                CertificateExpiry = Convert.ToDateTime(x509Certificate.GetExpirationDateString()),
                CertificateSubject = x509Certificate.Subject,
                HasPrivateKey = x509Certificate.HasPrivateKey,
                Issuer = x509Certificate.Issuer,
                FriendlyName = x509Certificate.FriendlyName,
                BasicConstraints = basicConstraints,
                SubjectAlternativeName = subjectAlternativeName,
                SerialNumber = x509Certificate.SerialNumber,
            };
        }

        public async Task<List<SecretContainer>> GetSecretContainers(string subscriptionId, string resourceGroupName, string cloudServiceName, string authenticationToken)
        {
            string azureContainerUrl = string.Format(_azureSecretContainerUrl, subscriptionId, resourceGroupName, cloudServiceName);

            _logger.LogInformation($"Retrieving secret containers from Azure API: {azureContainerUrl}.");

            // Call Azure API to get the secret containers

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);

                using (var request = new HttpRequestMessage(HttpMethod.Get, azureContainerUrl))
                {
                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode == false)
                        {
                            throw new HttpException($"ERROR retrieving secret container from Azure API. Status code: {response.StatusCode}");
                        }

                        string responseMessage = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"DEBUG: AzureSecretContainerService.GetSecretContainers - response message is: {responseMessage}.");

                        var cloudService = await response.Content.ReadAsAsync<dynamic>();
                        return cloudService.properties.osProfile.secrets.ToObject<List<SecretContainer>>() ?? new List<SecretContainer>();
                    }
                }
            }
        }
    }
}


