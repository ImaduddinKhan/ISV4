namespace IdentityServer.Infrastructure.Proxies
{
    public class CRMCreateUserRequest
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Interests { get; set; }
        public string Address { get; set; }
    }
}
