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
    public class RepoItemContextSeed
    {
        public async Task SeedAsync(
            RepoItemContext context, 
            IHostingEnvironment env, 
            IOptions<RepoItemSettings> settings, 
            ILogger<RepoItemContextSeed> logger)
        {
            var policy = CreatePolicy(logger, nameof(RepoItemContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                var useCustomizationData = settings.Value.UseCustomizationData;
                var contentRootPath = env.ContentRootPath;
                var picturePath = env.WebRootPath;

                if (!context.Repos.Any())
                {
                    await context.Repos.AddRangeAsync(useCustomizationData
                        ? GetDatasFromFile(contentRootPath, logger)
                        : GetPreconfiguredDatas());

                    await context.SaveChangesAsync();
                }
            });
        }

        private IEnumerable<RepoItem> GetDatasFromFile(
            string contentRootPath, 
            ILogger<RepoItemContextSeed> logger)
        {
            return GetPreconfiguredDatas();
        }

        private IEnumerable<RepoItem> GetPreconfiguredDatas()
        {
            return new List<RepoItem>()
            {
                new RepoItem(){ },
                new RepoItem(){ },
                new RepoItem(){ }
            };
        }

        private AsyncRetryPolicy CreatePolicy(
            ILogger<RepoItemContextSeed> logger, 
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
