using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificateInventory.Services
{
    public interface IAzureResourceService
    {
        public Task<T> GetResource<T>(string resourceUrl, string authenticationToken);
    }
}
