using System.Text.Json.Serialization;

namespace CertificateInventory.Repositories.Models
{
    public class KeyVault
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("name")]
        public string? VaultName { get; set; }
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("location")]
        public string? Location { get; set; }
        [JsonPropertyName("tags")]
        public Tags? Tags { get; set; }
    }
}
