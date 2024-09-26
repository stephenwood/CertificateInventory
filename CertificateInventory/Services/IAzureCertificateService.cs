using CertificateInventory.Repositories.Models;

namespace CertificateInventory.Services
{
    public interface IAzureCertificateService
    {        public Task WriteCertificate(CertificateMetadata certificateMetadata);
    }
}
