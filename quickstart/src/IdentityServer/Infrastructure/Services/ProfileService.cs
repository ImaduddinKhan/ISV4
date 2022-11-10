using IdentityServer.Infrastructure.DB;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Infrastructure.Services
{
    public class ProfileService : IProfileService
    {
        public UserManager<ApplicationUser> UserManager { get; }

        public ProfileService(UserManager<ApplicationUser> um)
        {
            UserManager = um;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var fClaims = context.Subject.Identities.FirstOrDefault();
            if (fClaims != null)
                context.IssuedClaims = context.Subject.Identities.First().Claims.ToList();

            var sub = fClaims.FindFirst("sub");
            if (sub != null)
            {
                var user = await UserManager.FindByNameAsync(sub.Value);
                if (user == null)
                    user = await UserManager.FindByIdAsync(sub.Value);

                if (context.Caller.ToLower().Contains("userinfo"))
                {
                    if (user != null)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim("preferred_username", user.UserName),
                            new Claim("email_verified", "false")
                        };

                        context.IssuedClaims.AddRange(claims);
                    }
                }
                else
                {
                    if (user != null)
                    {
                        var roles = await UserManager.GetRolesAsync(user);
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim("email", user.Email),
                            new Claim("someEntity", user.WhatEverEntity.ToString()),
                            //new Claim("Roles", string.Join(",", roles)),
                            new Claim("role", string.Join(",", roles))
                        };

                        foreach (var toBeRemoved in new[] { "phone_number", "email_verified", "phone_number_verified" })
                        {
                            var tbr_claim = context.IssuedClaims.FirstOrDefault(i => i.Type == toBeRemoved);
                            if (tbr_claim != null)
                                context.IssuedClaims.Remove(tbr_claim);
                        }

                        context.IssuedClaims.AddRange(claims);
                    }
                }
            }
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.FromResult(0);
        }
    }
}
