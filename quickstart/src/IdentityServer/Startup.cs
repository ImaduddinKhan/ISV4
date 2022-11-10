// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer.Certs;
using IdentityServer.Infrastructure.DB;
using IdentityServer.Infrastructure.Proxies;
using IdentityServer.Infrastructure.Services;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration config, IWebHostEnvironment environment)
        {
            _config = config;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<AspNetKeysDbContext>(options =>
                    options.UseNpgsql(_config["Data:DbContext:AspNetKeysConnectionString"]));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_config["Data:DbContext:UserStoreConnectionString"]));

            services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
                    options.Password = new PasswordOptions
                    {
                        RequiredLength = 8,
                        RequireNonAlphanumeric = false,
                        RequireDigit = true,
                        RequireLowercase = true,
                        RequireUppercase = true,
                    };
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string configStoreConnectionString = _config["Data:DbContext:ConfigStoreConnectionString"];
            string operationalStoretring = _config["Data:DbContext:OperationStoreConnectionString"];

            var crt = Certificate.Load(_config);
            var builder = services
                .AddIdentityServer(opt =>
                {
                    opt.IssuerUri = _config["AppSettings:issuerUri"];
                    opt.Authentication.CookieLifetime = TimeSpan.FromMinutes(20);
                    opt.AccessTokenJwtType = "JWT";
                    opt.EmitStaticAudienceClaim = true;
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddSigningCredential(crt)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(configStoreConnectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseNpgsql(operationalStoretring,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddProfileService<ProfileService>();

            IdentityModelEventSource.ShowPII = true;

            services.AddDataProtection()
                .SetApplicationName("Hala_IS")
                .PersistKeysToDbContext<AspNetKeysDbContext>();
            
            services.AddCors(o => o.AddPolicy("AllowAllPolicy", options =>
            {
                options.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddTransient<ICRMProxy, CRMProxy>();
        }

        public void Configure(IApplicationBuilder app)
        {

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
                           builder
                               .AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod());

            // uncomment if you want to add MVC
            app.UseForwardedHeaders();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            //InitializeDatabaseAsync(app).Wait();
        }

        private async Task InitializeDatabaseAsync(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var configurationDbContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                var persistantGrantContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
                var applicationDbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Log.Information("============== ApplicationDbContext EnsureCreatedAsync ===============");
                if (await applicationDbContext.Database.EnsureCreatedAsync())
                {
                    applicationDbContext.Database.Migrate();
                    applicationDbContext.Roles.Add(new IdentityRole("client"));
                    applicationDbContext.Roles.Add(new IdentityRole("admin"));
                    await applicationDbContext.SaveChangesAsync();
                }

                Log.Information("============== AspNetKeysContext EnsureCreatedAsync ===============");
                var AspNetKeysContext = serviceScope.ServiceProvider.GetRequiredService<AspNetKeysDbContext>();
                if (await AspNetKeysContext.Database.EnsureCreatedAsync())
                    AspNetKeysContext.Database.Migrate();


                Log.Information("============== PersistedGrantDbContext EnsureCreatedAsync ===============");
                if (await persistantGrantContext.Database.EnsureCreatedAsync())
                    persistantGrantContext.Database.Migrate();

                Log.Information("============== ConfigurationDbContext EnsureCreatedAsync ===============");
                if (await configurationDbContext.Database.EnsureCreatedAsync())
                    configurationDbContext.Database.Migrate();


                if (!(await configurationDbContext.Clients.AnyAsync()))
                {
                    foreach (var client in Config.Clients)
                    {
                        configurationDbContext.Clients.Add(client.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        configurationDbContext.IdentityResources.Add(resource.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.ApiResources.Any())
                {
                    foreach (var resource in Config.Apis)
                    {
                        configurationDbContext.ApiResources.Add(resource.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }

                if (!configurationDbContext.ApiScopes.Any())
                {
                    foreach (var scopes in Config.ApiScopes)
                    {
                        configurationDbContext.ApiScopes.Add(scopes.ToEntity());
                    }
                    configurationDbContext.SaveChanges();
                }
            }
        }
    }

}
