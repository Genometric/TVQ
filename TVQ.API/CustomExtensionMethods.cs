using Genometric.TVQ.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Genometric.TVQ.API
{
    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomDbContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<TVQContext>(
                options =>
                {
                    options
                    .UseLazyLoadingProxies(true)
                    .UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                            /// Configuring Connection Resiliency: 
                            /// https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 10,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        });
                });

            return services;
        }
    }
}
