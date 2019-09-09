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

                if (!context.RepoItems.Any())
                {
                    await context.RepoItems.AddRangeAsync(
                        GetPreconfiguredRepos(
                            contentRootPath,
                            useCustomizationData,
                            logger));

                    await context.SaveChangesAsync();
                }

                if (!context.ToolShedItems.Any())
                {
                    await context.ToolShedItems.AddRangeAsync(
                        GetPreconfiguredToolSheds(
                            contentRootPath,
                            useCustomizationData,
                            logger));

                    await context.SaveChangesAsync();
                }
            });
        }

        private IEnumerable<RepoItem> GetPreconfiguredRepos(
            string contentRootPath, 
            bool useCustomizationData, 
            ILogger<TVQContextSeed> logger)
        {
            return new List<RepoItem>()
            {
                new RepoItem(){ },
                new RepoItem(){ },
                new RepoItem(){ }
            };
        }

        private IEnumerable<ToolShedItem> GetPreconfiguredToolSheds(
            string contentRootPath, 
            bool useCustomizationData, 
            ILogger<TVQContextSeed> logger)
        {
            return new List<ToolShedItem>()
            {
                new ToolShedItem(){ },
                new ToolShedItem(){ },
                new ToolShedItem(){ }
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
