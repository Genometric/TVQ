using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Analysis
{
    public class AnalysisService
    {
        private readonly TVQContext _dbContext;
        private readonly ILogger<AnalysisService> _logger;

        public AnalysisService(
            TVQContext dbContext,
            ILogger<AnalysisService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task UpdateStatsAsync(Repository repository, CancellationToken cancellationToken)
        {

        }
    }
}
