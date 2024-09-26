
using System.Text.Json.Serialization;

namespace CertificateInventory.Repositories.Models
{
    public class KeyVaultCollection
    {
        [JsonPropertyName("value")]
        public List<KeyVault>? KeyVaults { get; set; }
        [JsonPropertyName("nextLink")]
        public string? NextLink { get; set; }
    }

    public class Tags
    {
    }
}
