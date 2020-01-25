using Genometric.TVQ.API.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public interface IBackgroundLiteratureCrawlingQueue
    {
        void QueueBackgroundWorkItem(LiteratureCrawlingJob job);

        Task<LiteratureCrawlingJob> DequeueAsync(CancellationToken cancellationToken);
    }
}
