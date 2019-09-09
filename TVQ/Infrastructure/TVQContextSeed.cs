using Genometric.TVQ.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.Infrastructure
{
    public class TVQContextSeed
    {
        public async Task SeedAsync(
            TVQContext context,
            IHostingEnvironment env,
            IOptions<TVQSettings> settings,
            ILogger<TVQContextSeed> logger)
        {
            var policy = CreatePolicy(logger, nameof(TVQContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                var useCustomizationData = settings.Value.UseCustomizationData;
                var contentRootPath = env.ContentRootPath;

                if (!context.Tools.Any())
                {
                    await context.Tools.AddRangeAsync(
                        GetPreconfiguredTools(
                            contentRootPath,
                            useCustomizationData,
                            logger));

                    await context.SaveChangesAsync();
                }

                if (!context.Repositories.Any())
                {
                    await context.Repositories.AddRangeAsync(
                        GetPreconfiguredRepositories(
                            contentRootPath,
                            useCustomizationData,
                            logger));

                    await context.SaveChangesAsync();
                }
            });
        }

        private IEnumerable<Tool> GetPreconfiguredTools(
            string contentRootPath, 
            bool useCustomizationData, 
            ILogger<TVQContextSeed> logger)
        {
            return new List<Tool>()
            {
                new Tool(){ },
                new Tool(){ },
                new Tool(){ }
            };
        }

        private IEnumerable<Repository> GetPreconfiguredRepositories(
            string contentRootPath, 
            bool useCustomizationData, 
            ILogger<TVQContextSeed> logger)
        {
            return new List<Repository>()
            {
                new Repository(){ URI = "https://toolshed.g2.bx.psu.edu/api/repositories" },
            };
        }

        private AsyncRetryPolicy CreatePolicy(
            ILogger<TVQContextSeed> logger, 
            string prefix, 
            int retries = 3)
        {
            return Policy.Handle<SqlException>().
                WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(
                            exception, 
                            "[{prefix}] Exception {ExceptionType} with message {Message} " +
                            "detected on attempt {retry} of {retries}", 
                            prefix, 
                            exception.GetType().Name, 
                            exception.Message, 
                            retry, 
                            retries);
                    }
                );
        }
    }
}
