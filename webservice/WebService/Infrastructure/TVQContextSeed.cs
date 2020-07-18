﻿using Genometric.TVQ.WebService.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Infrastructure
{
    public sealed class TVQContextSeed
    {
        public static async Task SeedAsync(
            TVQContext context,
            IWebHostEnvironment env,
            IOptions<Settings> settings,
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

                if (!context.Services.Any())
                {
                    await context.Services.AddRangeAsync(GetPreconfiguredServices())
                                          .ConfigureAwait(false);
                    saveRequired = true;
                }

                if (saveRequired)
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

            var bioconda = new Repository()
            {
                Name = Repository.Repo.Bioconda,
                URI = "https://github.com/VJalili/bioconda-recipes/archive/cheetah_template.zip"
            };
            bioconda.Statistics = new Statistics() { Repository = bioconda };

            return new List<Repository>()
            {
                toolshed, biotools, bioconductor, bioconda
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
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(6,
                                   retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                   onRetry: (exception, timeSpan, retry, ctx) =>
                                   {
                                       logger.LogWarning(exception,
                                                         "[{prefix}] Exception {ExceptionType} with message " +
                                                         "{Message} detected on attempt {retry} of {retries}",
                                                         prefix,
                                                         exception.GetType().Name,
                                                         exception.Message,
                                                         retry,
                                                         retries);
                                   });
        }
    }
}
