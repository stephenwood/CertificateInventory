
using System.Text.Json.Serialization;

namespace CertificateInventory.Repositories.Models.CloudServices
{
    public class CloudServiceCollection
    {
        [JsonPropertyName("value")]
        public List<CloudService>? CloudServices { get; set; }
    }

}
