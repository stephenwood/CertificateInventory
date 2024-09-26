using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace CertificateInventory.Services
{
    public class AzureAccessTokenService : IAzureAccessTokenService
    {
        private readonly ILogger _logger;
        private readonly DefaultAzureCredential _defaultAzureCredential;

        public AzureAccessTokenService(ILogger<object> logger)
        {
            this._logger = logger;
            this._defaultAzureCredential = new DefaultAzureCredential();
        }

        public async Task<string> GetToken(string resourceUrl)
        {
            _logger.LogInformation($"AzureAccessTokenService.GetToken: Getting access token for resource {resourceUrl}.");

            AccessToken accessToken = await _defaultAzureCredential.GetTokenAsync(new TokenRequestContext(new[] { resourceUrl }));
            return accessToken.Token;
        }
    }
}
