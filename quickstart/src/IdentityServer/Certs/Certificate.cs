using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer.Certs
{
    public class Certificate
    {
        public static X509Certificate2 Load(IConfiguration configuration)
        {
            var assembly = typeof(Certificate).Assembly;
            using (var stream = assembly.GetManifestResourceStream("IdentityServer.Certs.is4crt.pfx"))
            {
                return new X509Certificate2(ReadStream(stream), configuration["AppSettings:CertPassword"] ?? "P@ssw0rd", X509KeyStorageFlags.MachineKeySet);
            }
        }

        private static byte[] ReadStream(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }


}
