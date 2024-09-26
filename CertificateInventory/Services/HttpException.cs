using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CertificateInventory.Services
{
    public class HttpException:Exception
    {
        public HttpException(string message):base(message)
        {
        }
        public HttpException(string message, HttpStatusCode statusCode)
        {
        }

        public HttpException(string message, HttpStatusCode statusCode, Exception innerException):base(message, innerException)
        {
        }
    }
}
