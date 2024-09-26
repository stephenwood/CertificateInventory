using CertificateInventory.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateInventory.Services
{
    public interface IAzureAccessTokenService
    {
        Task<string> GetToken(string resourceUrl);
    }
}
