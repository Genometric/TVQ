using Genometric.TVQ.API.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public interface IBackgroundToolRepoCrawlingQueue
    {
        void QueueBackgroundWorkItem(RepoCrawlingJob job);

        Task<RepoCrawlingJob> DequeueAsync(CancellationToken cancellationToken);
    }
}
