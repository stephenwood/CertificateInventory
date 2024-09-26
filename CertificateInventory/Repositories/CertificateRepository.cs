
using CertificateInventory.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace CertificateInventory.Repositories
{
   using Microsoft.Extensions.Logging;

    public class CertificateRepository:ICertificateRepository
    {
        private readonly InventoryDbContext _dbContext;
        private readonly ILogger<CertificateRepository> _logger;

        public CertificateRepository(ILogger<CertificateRepository> logger, InventoryDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task CreateCertificate(CertificateMetadata certificateMetadata)
        {
            _logger.LogInformation($"CertificateRepository.CreateCertificate: executing.");

            await _dbContext.Certificates.AddAsync(certificateMetadata);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"CertificateRepository.CreateCertificate: completed!");
        }

        public async Task CreateCertificates(IEnumerable<CertificateMetadata> certificateMetadata)
        {
            _logger.LogInformation($"CertificateRepository.CreateCertificates: executing.");

            await _dbContext.Certificates.AddRangeAsync(certificateMetadata);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"CertificateRepository.CreateCertificates: completed!");
        }

        public async Task<bool> Exists(CertificateMetadata certificateMetadata)
        {
            // So we don't check for the existence of a certificate by its ID. We check by a combination of attributes.
            // This is a case, frankly, where we could have derived the primary key from the data, and we may want to revisit
            // the design to do that. For example, the combination of Serial Number and Issuer should guarantee
            // uniqueness.

            CertificateMetadata? metadata = await _dbContext.Certificates.FirstOrDefaultAsync(x =>
                    x.ResourceName == certificateMetadata.ResourceName &&
                    x.CertificateThumbprint == certificateMetadata.CertificateThumbprint &&
                    x.CertificateStore == certificateMetadata.CertificateStore && 
                    x.ScanType == certificateMetadata.ScanType || x.ScanType == null);
    
            if (metadata == null)
            {
                return false;
            }

            return true;
        }

        public Task<IEnumerable<CertificateMetadata>> GetCertificates()
        {
            throw new NotImplementedException();
        }
    }
}
