
namespace CertificateInventory.Repositories.Models
{
    public class SecretVersion
    {
        public SecretVersion(
            string Id,
            SecretVersionAttributeContainer Attributes)
        {
            id = Id;
            attributes = Attributes;
        }

        public string id { get; }
        public SecretVersionAttributeContainer attributes { get; }
    }
}
