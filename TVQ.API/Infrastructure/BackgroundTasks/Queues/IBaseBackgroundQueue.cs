using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public interface IBaseBackgroundTaskQueue<T>
    {
        void Enqueue(T job);

        Task<T> DequeueAsync(CancellationToken cancellationToken);
    }
}
