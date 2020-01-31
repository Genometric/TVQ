using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Analysis
{
    public class AnalysisService : BaseService<AnalysisJob>
    {
        // -------------------------------------------------
        // All the methods of this service are experimental, 
        // possibly with poor performance. All would benefit 
        // from a re-implementation.
        // -------------------------------------------------

        /// <summary>
        /// There are many articles that cite very old "related"
        /// works, which can bias the statistics. As a patch, 
        /// we set a earliest date for citations, and any reference
        /// earlier than that will be ignored. 
        /// </summary>
        private int _earliestCitationYear = 2000;

        public AnalysisService(
            TVQContext context,
            ILogger<AnalysisService> logger) :
            base(context, logger)
        { }

        protected override async Task ExecuteAsync(AnalysisJob job, CancellationToken cancellationToken)
        {
            if (job == null)
                return;

            /// TODO: the following should be broken down into multiple 	
            /// separate LINQ queries to avoid the Cartesian explosion problem.	
            job = Context.AnalysisJobs.Include(x => x.Repository)
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

            var repository = job.Repository;
            if (!Context.Repositories.Local.Any(e => e.ID == repository.ID))
                Context.Attach(repository);

            EvaluateCitationImpact(repository);
        }

        public Dictionary<int, List<CitationChange>> GetPrePostCitationCountNormalizedYear(
            Repository repository,
            HashSet<int> toolsToInclude = null)
        {
            int minCitationCount = int.MaxValue;
            int maxCitationCount = 0;
            int minBeforeDays = 0;
            int maxAfterDays = 0;
            var changes = new Dictionary<int, List<CitationChange>>();
            if (repository == null)
                return changes;
            foreach (var association in repository.ToolAssociations)
            {
                var tool = association.Tool;
                if (toolsToInclude != null &&
                    !toolsToInclude.Contains(tool.ID))
                    continue;
                Context.Entry(tool).Collection(x => x.Publications).Load();
                foreach (var pub in tool.Publications)
                    if (pub.Citations != null && pub.Year >= _earliestCitationYear)
                    {
                        Context.Entry(pub).Collection(x => x.Citations).Load();
                        foreach (var citation in pub.Citations)
                        {
                            if (!changes.ContainsKey(tool.ID))
                                changes.Add(tool.ID, new List<CitationChange>());

                            var daysOffset = (citation.Date - association.DateAddedToRepository).Value.Days;
                            minBeforeDays = Math.Min(minBeforeDays, daysOffset);
                            maxAfterDays = Math.Max(maxAfterDays, daysOffset);

                            changes[tool.ID].Add(new CitationChange(daysOffset, citation.Count));

                            minCitationCount = Math.Min(minCitationCount, citation.Count);
                            maxCitationCount = Math.Max(maxCitationCount, citation.Count);
                        }
                    }
            }

            // TODO: improve the following normalization, the iteration may not be optimal.
            var toolIDs = changes.Keys.ToList();
            foreach (var id in toolIDs)
            {
                var tool = changes[id];
                for (int i = 0; i < tool.Count; i++)
                {
                    tool[i].CitationCount = (tool[i].CitationCount - minCitationCount) / (maxCitationCount - minCitationCount);
                    if (tool[i].DaysOffset < 0)
                        tool[i].DaysOffset = (-1) - ((tool[i].DaysOffset - minBeforeDays) / minBeforeDays);
                    else
                        tool[i].DaysOffset = tool[i].DaysOffset / maxAfterDays;
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
                        post.Add(change.CitationCount);
                    else
                        pre.Add(change.CitationCount);
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

        private void EvaluateCitationImpact(Repository repository)
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
