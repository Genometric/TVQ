using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;

namespace Genometric.TVQ.WebService.BuildingBlocks.WebHost
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
                    logger.LogInformation($"Migrating database associated with context {typeof(TContext).Name}");

                    var retry = Policy
                        .Handle<Exception>()
                        .WaitAndRetry(5,
                                      retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                      onRetry: (exception, timeSpan, retry, ctx) =>
                                      {
                                          logger.LogWarning(exception,
                                                            "[{prefix}] Exception {ExceptionType} with " +
                                                            "message {Message} detected on attempt {retry} " +
                                                            "of {retries}",
                                                            nameof(IWebHostExtensions),
                                                            exception.GetType().Name,
                                                            exception.Message,
                                                            retry,
                                                            100);
                                      });

                    /// if the sql server container is not created on run docker
                    /// compose this migration can't fail for network related
                    /// exception. The retry options for DbContext only apply
                    /// to transient exceptions.
                    /// Note that this is NOT applied when running some
                    /// orchestrator (let the orchestrator to recreate the
                    /// failing service)
                    retry.Execute(() => InvokeSeeder(seeder, context, services));

                    logger.LogInformation($"Migrated database associated with context {typeof(TContext).Name}");
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
            context.Database.Migrate();
            seeder(context, services);
        }
    }
}
