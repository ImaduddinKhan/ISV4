using System.Threading.Tasks;

namespace IdentityServer.Infrastructure.Proxies
{
    public interface ICRMProxy
    {
        public Task<CRMCreateUserResponse> CreateCRMUser(CRMCreateUserRequest requestBody);
    }
}
