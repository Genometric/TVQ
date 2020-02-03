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
            HashSet<int> toolsToInclude = null,
            bool normalizeCount = true)
        {
            double minCitationCount = int.MaxValue;
            double maxCitationCount = 0;
            double minBeforeDays = 0;
            double maxAfterDays = 0;
            double minBeforeYears = 0;
            double maxAfterYears = 0;
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
                            var yearsOffset = citation.Date.Year - association.DateAddedToRepository.Value.Year;
                            minBeforeDays = Math.Min(minBeforeDays, daysOffset);
                            minBeforeYears = Math.Min(minBeforeYears, yearsOffset);

                            maxAfterDays = Math.Max(maxAfterDays, daysOffset);
                            maxAfterYears = Math.Max(maxAfterYears, yearsOffset);

                            changes[tool.ID].Add(new CitationChange(daysOffset, citation.Count)
                            {
                                YearsOffset = yearsOffset
                            });

                            // minCitationCount = Math.Min(minCitationCount, citation.Count);
                            // maxCitationCount = Math.Max(maxCitationCount, citation.Count);
                        }
                    }
            }


            // First normalize citation count w.r.t to date. 
            var toolIDs = changes.Keys.ToList();

            minCitationCount = int.MaxValue;
            maxCitationCount = 0;
            foreach (var id in toolIDs)
            {
                var tool = changes[id];
                for (int i = 0; i < tool.Count; i++)
                {
                    if (tool[i].DaysOffset <= 0)
                        tool[i].CitationCount /= Math.Abs(minBeforeDays);
                    else
                        tool[i].CitationCount /= maxAfterDays;

                    minCitationCount = Math.Min(minCitationCount, tool[i].CitationCount);
                    maxCitationCount = Math.Max(maxCitationCount, tool[i].CitationCount);
                }
            }


            // Second, normalize citation count and date to using min-max normalization. 
            foreach (var id in toolIDs)
            {
                var tool = changes[id];
                for (int i = 0; i < tool.Count; i++)
                {
                    if (normalizeCount)
                        tool[i].CitationCount = (tool[i].CitationCount - minCitationCount) / (maxCitationCount - minCitationCount);
                    if (tool[i].DaysOffset < 0)
                    {
                        tool[i].DaysOffset = (-1) - ((tool[i].DaysOffset - minBeforeDays) / minBeforeDays);
                        tool[i].YearsOffset = (-1) - ((tool[i].YearsOffset - minBeforeYears) / minBeforeYears);
                    }
                    else
                    {
                        tool[i].DaysOffset = tool[i].DaysOffset / maxAfterDays;
                        tool[i].YearsOffset = tool[i].YearsOffset / maxAfterYears;
                    }
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

        public List<double> GetDeltaPrePostCitationChanges(Repository repository)
        {
            var changes = GetPrePostCitationCountNormalizedYear(repository);
            double sumPre, sumPost, countPre, countPost;
            var deltas = new List<double>();
            if (changes == null)
                return deltas;

            foreach (var tool in changes)
            {
                sumPre = sumPost = countPre = countPost = 0;
                foreach (var change in tool.Value)
                {
                    if (change.DaysOffset < 0)
                    {
                        sumPre += change.CitationCount;
                        countPre += 1;
                    }
                    else
                    {
                        sumPost += change.CitationCount;
                        countPost += 1;
                    }
                }

                var averagePre = sumPre / countPre;
                var averagePost = sumPost / countPost;

                if (double.IsNaN(averagePre) || double.IsInfinity(averagePre))
                    averagePre = 0;
                if (double.IsNaN(averagePost) || double.IsInfinity(averagePost))
                    averagePost = 0;

                deltas.Add(Math.Abs(averagePost - averagePre));
            }

            return deltas;
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

        public IEnumerable<CitationChange> GetPrePostCitationChangeVector(Repository repository)
        {
            var rtv = new SortedDictionary<double, CitationChange>();
            var tools = GetPrePostCitationCountNormalizedYear(repository, normalizeCount: false);

            double offset;
            foreach (var tool in tools)
            {
                foreach (var change in tool.Value)
                {
                    if (change.CitationCount == 0)
                        continue;


                    var binCount = 20;
                    double end = 1.0, start = -1.0;
                    var range = end - start;
                    var length = range / binCount;
                    var binNumber = Math.Floor((change.DaysOffset + 1) / length);
                    offset = (binNumber * length) - 1;


                    // offset = Math.Round(change.DaysOffset * 10.0) / 10.0;

                    if (rtv.ContainsKey(offset))
                    {
                        rtv[offset].AddCitationCount(change.CitationCount);
                    }
                    else
                    {
                        var c = new CitationChange();
                        c.AddCitationCount(change.CitationCount);
                        c.YearsOffset = change.YearsOffset;
                        c.DaysOffset = offset;
                        c.CitationCount = change.CitationCount;
                        rtv.Add(offset, c);
                    }
                }
            }

            double minCitationCount = int.MaxValue;
            double maxCitationCount = 0;
            foreach (var item in rtv)
            {
                item.Value.RemoveOutliers();
                minCitationCount = Math.Min(minCitationCount, item.Value.Min);
                maxCitationCount = Math.Max(maxCitationCount, item.Value.Max);
            }

            foreach (var item in rtv)
                item.Value.MinMaxNormalize(minCitationCount, maxCitationCount);

            return rtv.Values;
        }

        private void EvaluateCitationImpact(Repository repository)
        {
            // This method is for paired difference test
            var deltas = GetDeltaPrePostCitationChanges(repository);
            var sigDiff = InferentialStatistics.ComputeTTest(
                deltas,
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
