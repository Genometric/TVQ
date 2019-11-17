using Genometric.TVQ.API.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class QueuedAnalysis : BackgroundService
    {
        private ILogger<QueuedAnalysis> _logger;
        private IServiceProvider Services { get; }
        public IBackgroundAnalysisTaskQueue AnalysisQueue { get; }

        public QueuedAnalysis(
            IBackgroundAnalysisTaskQueue analysisQueue,
            IServiceProvider services,
            ILogger<QueuedAnalysis> logger)
        {
            Services = services;
            _logger = logger;
            AnalysisQueue = analysisQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var repository = await AnalysisQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    using var scope = Services.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<AnalysisService>().UpdateStatsAsync(repository, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                       $"Error occurred executing {nameof(repository)}.");
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
