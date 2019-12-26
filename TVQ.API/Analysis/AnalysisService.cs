using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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
            if (repository == null) return;

            if (!_dbContext.Repositories.Local.Any(e => e.ID == repository.ID))
                _dbContext.Attach(repository);

            EvaluateCitationImpact(repository);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private void EvaluateCitationImpact(Repository repository)
        {
            var citations = new Dictionary<int, double[]>();
            foreach (var tool in repository.Tools)
                foreach (var pub in tool.Publications)
                {
                    if (!citations.ContainsKey(tool.ID))
                        citations.Add(tool.ID, new double[2]);

                    if (pub.Citations != null)
                        foreach (var citation in pub.Citations)
                            if (citation.Date < tool.DateAddedToRepository)
                            {
                                citations[tool.ID][0] += citation.Count;
                                citations[tool.ID][1] += citation.Count;
                            }
                            else
                            {
                                citations[tool.ID][1] += citation.Count;
                            }
                }

            var sigDiff = InferentialStatistics.ComputeTTest(
                citations.Values.Select(x => x[0]).ToList(),
                citations.Values.Select(x => x[1]).ToList(),
                0.05,
                out double df,
                out double tScore,
                out double pValue,
                out double criticalValue);

            repository.Statistics.TScore = tScore;
            repository.Statistics.PValue = pValue;

        }
    }
}
