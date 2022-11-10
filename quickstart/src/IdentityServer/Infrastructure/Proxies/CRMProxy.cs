using System;
using System.Threading.Tasks;

namespace IdentityServer.Infrastructure.Proxies
{
    public class CRMProxy : ICRMProxy
    {
        public async Task<CRMCreateUserResponse> CreateCRMUser(CRMCreateUserRequest requestBody)
        {
            return new CRMCreateUserResponse()
            {
                UserID = DateTime.Now.Ticks.ToString()
            };
        }
    }
}
