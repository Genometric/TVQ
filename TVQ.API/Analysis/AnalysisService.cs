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

        private void GetPrePostCitationCountPerYear(Repository repository, out List<double> pre, out List<double> post)
        {
            pre = new List<double>();
            post = new List<double>();
            double count;
            foreach (var association in repository.ToolAssociations)
            {
                var tool = association.Tool;
                foreach (var pub in tool.Publications)
                {
                    if (pub.Citations != null)
                        foreach (var citation in pub.Citations)
                        {
                            count = citation.Count / 12;
                            if (citation.Date < association.DateAddedToRepository)
                                pre.Add(count);
                            else
                                post.Add(count);
                        }
                }
            }
        }

        private void GetSumOfPrePostCitationsCount(Repository repository, out List<double> pre, out List<double> post)
        {
            var citations = new Dictionary<int, double[]>();
            foreach (var association in repository.ToolAssociations)
            {
                var tool = association.Tool;
                foreach (var pub in tool.Publications)
                {
                    if (!citations.ContainsKey(tool.ID))
                        citations.Add(tool.ID, new double[2]);

                    if (pub.Citations != null)
                        foreach (var citation in pub.Citations)
                            if (citation.Date < association.DateAddedToRepository)
                            {
                                citations[tool.ID][0] += citation.Count;
                                citations[tool.ID][1] += citation.Count;
                            }
                            else
                            {
                                citations[tool.ID][1] += citation.Count;
                            }
                }
            }

            pre = citations.Values.Select(x => x[0]).ToList();
            post = citations.Values.Select(x => x[1]).ToList();
        }

        private void EvaluateCitationImpact(Repository repository)
        {
            GetPrePostCitationCountPerYear(repository, out List<double> pre, out List<double> post);

            var sigDiff = InferentialStatistics.ComputeTTest(
                pre, post,
                0.05,
                out double df,
                out double tScore,
                out double pValue,
                out double criticalValue);

            repository.Statistics.TScore = tScore;
            repository.Statistics.PValue = pValue;
            repository.Statistics.DegreeOfFreedom = df;
            repository.Statistics.CriticalValue = criticalValue;
            repository.Statistics.MeansSignificantlyDifferent = sigDiff;
        }
    }
}
