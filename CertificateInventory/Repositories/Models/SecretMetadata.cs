using System.Text.Json.Serialization;

namespace CertificateInventory.Repositories.Models
{
    public class SecretMetadata
    {
        [JsonPropertyName("content_type")]
        public string? ContentType{ get; set; }
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("attributes")]
        public IEnumerable<SecretAttributes>? Attributes { get; set; }

    }
}
