using Genometric.TVQ.API.Analysis;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public class AnalysisJobRunner : BaseJobRunner<AnalysisJob>
    {
        public AnalysisJobRunner(
            TVQContext context,
            IServiceProvider services,
            ILogger<AnalysisJobRunner> logger,
            IBaseBackgroundTaskQueue<AnalysisJob> queue) :
            base(context,
                 services,
                 logger,
                 queue)
        { }

        protected override IQueryable<AnalysisJob> GetPendingJobs()
        {
            return Context.AnalysisJobs.Include(x => x.Repository)
                                       .Where(x => x.Status == State.Queued ||
                                                   x.Status == State.Running);
        }

        protected override AnalysisJob AugmentJob(AnalysisJob job)
        {
            return Context.AnalysisJobs
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
                .First(x => x.ID == job.ID);
        }

        protected override Task RunJobAsync(
            IServiceScope scope,
            AnalysisJob job,
            CancellationToken cancellationToken)
        {
            return scope.ServiceProvider.GetRequiredService<AnalysisService>()
                                        .UpdateStatsAsync(job, cancellationToken);
        }
    }
}
