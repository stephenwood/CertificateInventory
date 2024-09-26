using System.Text.Json.Serialization;

namespace CertificateInventory.Repositories.Models.CloudServices
{
    public class CloudService
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("location")]
        public string? Location { get; set; }
        [JsonPropertyName("systemData")]
        public SystemData? systemData { get; set; }
        [JsonPropertyName("tags")]
        public Tags? Tags { get; set; }
    }

    public class SystemData
    {
    }

    public class Tags
    {
        public string? environment { get; set; }
        public string? servicename { get; set; }
        public string? buildNumber { get; set; }
        public string? deploymenttype { get; set; }
    }

}
