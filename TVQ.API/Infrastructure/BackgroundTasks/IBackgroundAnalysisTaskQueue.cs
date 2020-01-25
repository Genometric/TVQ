using Genometric.TVQ.API.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public interface IBackgroundAnalysisTaskQueue
    {
        void QueueBackgroundWorkItem(AnalysisJob repository);

        Task<AnalysisJob> DequeueAsync(CancellationToken cancellationToken);
    }
}
