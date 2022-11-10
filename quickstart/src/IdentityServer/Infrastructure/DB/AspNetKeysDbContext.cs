using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Infrastructure.DB
{
    public class AspNetKeysDbContext : DbContext, IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public AspNetKeysDbContext(DbContextOptions<AspNetKeysDbContext> options) : base(options)
        {
        }
    }
}
