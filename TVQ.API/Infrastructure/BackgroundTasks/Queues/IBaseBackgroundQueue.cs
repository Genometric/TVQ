using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Infrastructure.BackgroundTasks
{
    public interface IBaseBackgroundTaskQueue<T>
    {
        void Enqueue(int id);

        Task<int> DequeueAsync(CancellationToken cancellationToken);
    }
}
