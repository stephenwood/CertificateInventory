using CertificateInventory.Repositories.Models;

namespace CertificateInventory.Repositories
{
    public interface ICertificateRepository
    {
        public Task CreateCertificate(CertificateMetadata certificateMetadata);
        public Task CreateCertificates(IEnumerable<CertificateMetadata> certificateMetadata);
        public Task<IEnumerable<CertificateMetadata>> GetCertificates();
        public Task<bool> Exists(CertificateMetadata certificateMetadata);
    }
}
