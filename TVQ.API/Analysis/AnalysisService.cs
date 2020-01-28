﻿using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Analysis
{
    public class AnalysisService : BaseService<AnalysisJob>
    {
        public AnalysisService(
            TVQContext context,
            ILogger<AnalysisService> logger) :
            base(context, logger)
        { }

        protected override async Task RunAsync(AnalysisJob job, CancellationToken cancellationToken)
        {
            if (job == null)
                return;

            var repository = job.Repository;
            if (!Context.Repositories.Local.Any(e => e.ID == repository.ID))
                Context.Attach(repository);

            job.Status = State.Running;
            EvaluateCitationImpact(repository);
            job.Status = State.Completed;
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public static Dictionary<int, List<CitationChange>> GetPrePostCitationCountNormalizedYear(Repository repository)
        {
            var changes = new Dictionary<int, List<CitationChange>>();
            foreach (var association in repository.ToolAssociations)
            {
                var tool = association.Tool;
                foreach (var pub in tool.Publications)
                    if (pub.Citations != null && pub.Year >= 2000)
                        foreach (var citation in pub.Citations)
                        {
                            if (!changes.ContainsKey(tool.ID))
                                changes.Add(tool.ID, new List<CitationChange>());

                            changes[tool.ID].Add(
                                new CitationChange(
                                    (citation.Date - association.DateAddedToRepository).Value.Days,
                                    citation.Count));
                        }

            }
            return changes;
        }

        public static void ExtractPrePostCitationChanges(Dictionary<int, List<CitationChange>> changes, out List<double> pre, out List<double> post)
        {
            pre = new List<double>();
            post = new List<double>();
            foreach (var tool in changes)
                foreach (var change in tool.Value)
                    if (change.DaysOffset > 0)
                        post.Add(change.Count);
                    else
                        pre.Add(change.Count);
        }

        public static void GetPrePostCitationCountPerYear(Repository repository, out List<double> pre, out List<double> post)
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

        public static void GetSumOfPrePostCitationsCount(Repository repository, out List<double> pre, out List<double> post)
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

        private static void EvaluateCitationImpact(Repository repository)
        {
            var changes = GetPrePostCitationCountNormalizedYear(repository);
            ExtractPrePostCitationChanges(changes, out List<double> pre, out List<double> post);

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
