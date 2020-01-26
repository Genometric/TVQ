using Genometric.TVQ.API.Analysis;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class QueuedAnalysis : BackgroundService
    {
        private TVQContext Context { get; }
        private ILogger<QueuedAnalysis> _logger;
        private IServiceProvider Services { get; }
        public IBaseBackgroundTaskQueue<AnalysisJob> AnalysisQueue { get; }

        public QueuedAnalysis(
            TVQContext context,
            IBaseBackgroundTaskQueue<AnalysisJob> analysisQueue,
            IServiceProvider services,
            ILogger<QueuedAnalysis> logger)
        {
            Context = context;
            Services = services;
            _logger = logger;
            AnalysisQueue = analysisQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            var unfinishedJobs = Context.AnalysisJobs.Include(x => x.Repository)
                .Where(x => x.Status == Model.State.Queued ||
                            x.Status == Model.State.Running);

            foreach (var job in unfinishedJobs)
            {
                AnalysisQueue.Enqueue(job);
                _logger.LogInformation($"The unfinished literature crawling job {job.ID} is re-queued.");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var dequeuedJob = await AnalysisQueue.DequeueAsync(cancellationToken).ConfigureAwait(false);

                /// TODO: This is a very long query, and it can 
                /// cause Cartesian explosion problem. 
                var job = Context.AnalysisJobs
                    .Include(x => x.Repository)
                        .ThenInclude(x => x.ToolAssociations)
                            .ThenInclude(x => x.Tool)
                    .Include(x => x.Repository)
                        .ThenInclude(x => x.ToolAssociations)
                            .ThenInclude(x => x.Downloads)
                    .Include(x => x.Repository)
                        .ThenInclude(x => x.ToolAssociations)
                            .ThenInclude(x => x.Tool)
                                .ThenInclude(x => x.Publications)
                                    .ThenInclude(x => x.Citations)
                    .Include(x => x.Repository)
                        .ThenInclude(x => x.Statistics)
                    .First(x => x.ID == dequeuedJob.ID);

                try
                {
                    using var scope = Services.CreateScope();
                    await
                        scope.ServiceProvider.GetRequiredService<AnalysisService>()
                        .UpdateStatsAsync(dequeuedJob, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                       $"Error occurred executing {nameof(dequeuedJob)}.");
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
