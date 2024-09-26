using CertificateInventory.Repositories.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace CertificateInventory.Services
{
    public class AzureCertificateService : IAzureCertificateService
    {
     //   private readonly string _secretContainersResourceUrl = "https://management.azure.com/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroupName}/providers/Microsoft.Compute/cloudServices/{CloudServiceName}?api-version=2022-04-04";
        private readonly ILogger<AzureCertificateService> _logger;

        public AzureCertificateService(ILogger<AzureCertificateService> logger)
        {
            _logger = logger;
  
        }


        public Task WriteCertificate(CertificateMetadata certificateMetadata)
        {
            throw new NotImplementedException();
        }
    }
}
