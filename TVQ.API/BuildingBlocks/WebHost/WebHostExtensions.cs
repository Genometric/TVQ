using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading;

namespace Genometric.TVQ.API.BuildingBlocks.WebHost
{
    public static class IWebHostExtensions
    {
        public static IWebHost MigrateDbContext<TContext>(
            this IWebHost webHost,
            Action<TContext, IServiceProvider> seeder)
            where TContext : DbContext
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    InvokeSeeder(seeder, context, services);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        $"An error occurred while migrating the database used on " +
                        $"context {typeof(TContext).Name}");
                    throw;
                }
            }

            return webHost;
        }

        private static void InvokeSeeder<TContext>(
            Action<TContext, IServiceProvider> seeder,
            TContext context,
            IServiceProvider services)
            where TContext : DbContext
        {
            /// TODO: a better way of doing this is using Polly.
            var logger = services.GetRequiredService<ILogger<TContext>>();
            int retryAttempt = 1;
            logger.LogInformation($"Migrating database associated with context {typeof(TContext).Name}.");
            string error = "";
            do
            {
                try
                {
                    context.Database.Migrate();
                    seeder(context, services);
                    logger.LogInformation($"Migrated database associated with context {typeof(TContext).Name}.");
                }
                catch(SqlException e)
                {
                    Thread.Sleep(Convert.ToInt32(Math.Pow(2, retryAttempt) * 1000));
                    error = e.Message;
                }
            } while (retryAttempt++ < 8);

            throw new Exception(error);
        }
    }
}
