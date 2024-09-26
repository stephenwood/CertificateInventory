
namespace CertificateInventory.Repositories.Models
{
    public class SecretVersionAttributeContainer
    {
        public SecretVersionAttributeContainer(
            bool Enabled,
            int Created,
            int Updated,
            string RecoveryLevel,
            int RecoverableDays)
        {
            enabled = Enabled;
            created = Created;
            updated = Updated;
            recoveryLevel = RecoveryLevel;
            recoverableDays = RecoverableDays;
        }

        public bool enabled { get; }
        public int created { get; }
        public int updated { get; }
        public string recoveryLevel { get; }
        public int recoverableDays { get; }
    }

}
