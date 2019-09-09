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
    public class ToolShedItemContextSeed
    {
        public async Task SeedAsync(
            ToolShedItemContext context,
            IHostingEnvironment env,
            IOptions<ToolShedItemSettings> settings,
            ILogger<ToolShedItemContextSeed> logger)
        {
            var policy = CreatePolicy(logger, nameof(ToolShedItemContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                var useCustomizationData = settings.Value.UseCustomizationData;
                var contentRootPath = env.ContentRootPath;
                var picturePath = env.WebRootPath;

                if (!context.ToolSheds.Any())
                {
                    await context.ToolSheds.AddRangeAsync(useCustomizationData
                        ? GetDatasFromFile(contentRootPath, logger)
                        : GetPreconfiguredDatas());

                    await context.SaveChangesAsync();
                }
            });
        }

        private IEnumerable<ToolShedItem> GetDatasFromFile(
            string contentRootPath,
            ILogger<ToolShedItemContextSeed> logger)
        {
            return GetPreconfiguredDatas();
        }

        private IEnumerable<ToolShedItem> GetPreconfiguredDatas()
        {
            return new List<ToolShedItem>()
            {
                new ToolShedItem(){ },
                new ToolShedItem(){ },
                new ToolShedItem(){ }
            };
        }

        private AsyncRetryPolicy CreatePolicy(
            ILogger<ToolShedItemContextSeed> logger,
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
