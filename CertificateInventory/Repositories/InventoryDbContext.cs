using CertificateInventory.Repositories.Models;
using CertificateInventory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CertificateInventory.Repositories
{
    public class InventoryDbContext: DbContext
    {
        private readonly DbContextOptions<InventoryDbContext> _options;
        public DbSet<CertificateMetadata> Certificates{ get; set; } = null!;
        private readonly ILogger<InventoryDbContext> _logger;

        public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ILogger<InventoryDbContext> logger) : base(options)
        {
            _options = options;
            _logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var task = Task.Run(async () => await GetConnectionString());
            var connectionString = task.Result;

            _logger.LogInformation($"DEBUG: Connection string is {connectionString}.");

            optionsBuilder.UseSqlServer(connectionString);
        }

        protected async Task<string> GetConnectionString()
        {
            _logger.LogInformation("InventoryDbContext.GetConnectionString: Getting connection string from Azure Key Vault.");

            //// Retrieve the SQL Server connection string from Azure Key Vault.

            string sqlConnectionSecretFilter = System.Environment.GetEnvironmentVariable("SqlConnectionSecretFilter", EnvironmentVariableTarget.Process) ?? throw new NullReferenceException("SqlConnectionSecretFilter environment variable not set.");
            string sqlConnectionKeyVaultName = System.Environment.GetEnvironmentVariable("SqlConnectionKeyVaultName", EnvironmentVariableTarget.Process) ?? throw new NullReferenceException("SqlConnectionKeyVaultName environment variable not set.");

            AzureResourceService azureResourceService = new AzureResourceService(new LoggerFactory().CreateLogger<AzureResourceService>());

            AzureAccessTokenService azureAccessTokenService = new AzureAccessTokenService(new LoggerFactory().CreateLogger<AzureAccessTokenService>());
            string azureKeyVaultToken = await azureAccessTokenService.GetToken("https://vault.azure.net");

            AzureKeyVaultService azureKeyVaultService = new AzureKeyVaultService(new LoggerFactory().CreateLogger<AzureKeyVaultService>(), azureResourceService);
            IEnumerable<SecretVersion> secretVersions = await azureKeyVaultService.GetSecretVersions(sqlConnectionKeyVaultName, sqlConnectionSecretFilter, azureKeyVaultToken);

            SecretVersion? secretVersion = secretVersions?.MaxBy(x => x.attributes.created);
            string? sqlConnectionSecretVersion = secretVersion?.id;

            sqlConnectionSecretVersion = sqlConnectionSecretVersion?.Split('/')[sqlConnectionSecretVersion.Split('/').Length - 1];

            string resourceUrl = $"https://{sqlConnectionKeyVaultName.ToLower()}.vault.azure.net/secrets/{sqlConnectionSecretFilter}/{sqlConnectionSecretVersion}?api-version=7.3";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + azureKeyVaultToken);

                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, resourceUrl))
                {
                    using (HttpResponseMessage response = await client.SendAsync(requestMessage))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"ERROR: Call to {resourceUrl} returned HTTP status code {response.StatusCode}.");
                        }

                        var responseContent = await response.Content.ReadAsAsync<dynamic>();
                        string sqlConnectionString = responseContent.value.ToObject<string>();

                        _logger.LogInformation($"InventoryDbContext.GetConnectionString: Connection string retrieved successfully!");

                        return sqlConnectionString;
                    }
                }
            }
                
        }
    }
}