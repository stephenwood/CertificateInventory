
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertificateInventory.Repositories.Models
{
    public class CertificateMetadata
    {
        [Column("AadApplicationId", TypeName = "VARCHAR(max) NULL")]
        public string? AadApplicationId { get; set; }

        [Column("AadApplicationObjectId", TypeName = "VARCHAR(max) NULL")]
        public string? AadApplicationObjectId { get; set; }

        [Column("AadApplicationOwners", TypeName = "VARCHAR(max) NULL")]
        public string? AadApplicationOwners { get; set; }

        [Column("BasicConstraints", TypeName = "nvarchar(max)")]
        public string? BasicConstraints { get; set; }

        [Column("CertificateExpiry")]
        public DateTime CertificateExpiry { get; set; }

        [Column("CertificateStore", TypeName = "nvarchar(255)")]
        public string? CertificateStore { get; set; }

        [Column("CertificateSubject", TypeName = "nvarchar(max)")]
        public string? CertificateSubject { get; set; }

        [Column("CertificateThumbprint", TypeName = "nvarchar(255)")]
        public string? CertificateThumbprint { get; set; }

        [Column("CreatedUtcDateTime")]
        public DateTime CreatedUtcDateTime { get; set; }

        [Column("FriendlyName", TypeName = "nvarchar(max)")]
        public string? FriendlyName { get; set; }

        [Column("HasPrivateKey")]
        public bool HasPrivateKey { get; set; }

        [Column("Id")]
        [Key]
        public long Id { get; set; }

        [Column("IsInstalled")]
        public bool IsInstalled { get; set; }

        [Column("Issuer", TypeName = "nvarchar(max)")]
        public string? Issuer { get; set; }

        [Column("LastDetectedInstalledUtcDateTime")]
        public DateTime LastDetectedInstalledUtcDateTime { get; set; }

        [Column("LastInventoriedUtcDateTime")]
        public DateTime LastInventoriedUtcDateTime { get; set; }

        [Column("Region", TypeName = "nvarchar(50)")]
        public string? Region { get; set; }

        [Column("ResourceAzureSubscription", TypeName = "nvarchar(255)")]
        public string? ResourceAzureSubscription { get; set; }

        [Column("ResourceEndpoint", TypeName = "nvarchar(max)")]
        public string? ResourceEndpoint { get; set; }

        [Column("ResourceEnvironment", TypeName = "nvarchar(50)")]
        public string? ResourceEnvironment { get; set; }

        [Column("ResourceName", TypeName = "nvarchar(255)")]
        public string? ResourceName { get; set; }

        [Column("ResourceType", TypeName = "nvarchar(255)")]
        public string? ResourceType { get; set; }

        [Column("ScanType", TypeName = "nvarchar(max)")]
        public string? ScanType { get; set; }

        [Column("SubjectAlternativeName", TypeName = "nvarchar(max)")]
        public string? SubjectAlternativeName { get; set; }

        [Column("SerialNumber", TypeName = "nvarchar(255)")]
        public string? SerialNumber { get; set; }

    }
}
