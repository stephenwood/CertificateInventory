
namespace CertificateInventory.Repositories.Models
{
    public class VaultCertificate
    {
        public VaultCertificate(string CertificateUrl)
        {
            certificateUrl = CertificateUrl;
        }
        public string certificateUrl { get; set; }
    }
}
