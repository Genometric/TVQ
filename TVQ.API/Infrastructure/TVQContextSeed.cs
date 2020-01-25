using Genometric.TVQ.API.Model;
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

namespace Genometric.TVQ.API.Infrastructure
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
                bool saveRequired = false;
                if (!context.Repositories.Any())
                {
                    await context.Repositories.AddRangeAsync(GetPreconfiguredRepositories())
                                              .ConfigureAwait(false);
                    saveRequired = true;
                }

                if(!context.Services.Any())
                {
                    await context.Services.AddRangeAsync(GetPreconfiguredServices())
                                          .ConfigureAwait(false);
                    saveRequired = true;
                }

                if(saveRequired)
                    await context.SaveChangesAsync().ConfigureAwait(false);

            }).ConfigureAwait(false);
        }

        private static IEnumerable<Repository> GetPreconfiguredRepositories()
        {
            var toolshed = new Repository()
            {
                Name = Repository.Repo.ToolShed,
                URI = "https://toolshed.g2.bx.psu.edu/api/"
            };
            toolshed.Statistics = new Statistics() { Repository = toolshed };

            var biotools = new Repository()
            {
                Name = Repository.Repo.BioTools,
                URI = "https://github.com/bio-tools/content/archive/master.zip"
            };
            biotools.Statistics = new Statistics() { Repository = biotools };

            var bioconductor = new Repository()
            {
                Name = Repository.Repo.Bioconductor,
                URI = "https://github.com/Genometric/TVQ/raw/master/data/bioconductor/"
            };
            bioconductor.Statistics = new Statistics() { Repository = bioconductor };

            return new List<Repository>()
            {
                toolshed, biotools, bioconductor
            };
        }

        private static IEnumerable<Service> GetPreconfiguredServices()
        {
            var services = new List<Service>();
            foreach (var type in (Service.Type[])Enum.GetValues(typeof(Service.Type)))
                services.Add(new Service() { Name = type });

            return services;
        }

        private static AsyncRetryPolicy CreatePolicy(
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
