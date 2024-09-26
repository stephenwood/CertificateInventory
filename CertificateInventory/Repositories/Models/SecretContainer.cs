
namespace CertificateInventory.Repositories.Models
{
    public class SecretContainer
    {
        public SecretContainer(
            SourceVault SourceVault,
            List<VaultCertificate> VaultCertificates)
        {
            sourceVault = SourceVault;
            vaultCertificates = VaultCertificates;
        }
        public SourceVault sourceVault { get; }
        public List<VaultCertificate> vaultCertificates { get; }
    }
}
