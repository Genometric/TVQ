using Genometric.TVQ.WebService.Infrastructure;
using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners;
using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.Associations;
using MathNet.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Analysis
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
        public int EarliestPublicationYear { set; get; } = 2000;

        /// <summary>
        /// Sets and gets minimum year offset (inclusive) between the 
        /// publication year and when the tool was added to a repository. 
        /// For instance, this ignore publications that were published
        /// at the same year as the tool was added to the repository.
        /// </summary>
        public int MinPubDateAndDateAddedToRepoOffset { set; get; } = 1;

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
                                                .ThenInclude(x => x.PublicationAssociations)
                                                    .ThenInclude(x => x.Publication.Citations)
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
                Context.Entry(tool).Collection(x => x.PublicationAssociations).Load();
                foreach (var publicationAssociation in tool.PublicationAssociations)
                {
                    //Context.Entry(pub).Collection(x => x.Publication).Load();
                    var publication = publicationAssociation.Publication;
                    if (publication.Citations != null && publication.Year >= EarliestPublicationYear)
                    {
                        Context.Entry(publication).Collection(x => x.Citations).Load();
                        foreach (var citation in publication.Citations)
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
                foreach (var publicationAssociation in tool.PublicationAssociations)
                {
                    if (publicationAssociation.Publication.Citations != null)
                        foreach (var citation in publicationAssociation.Publication.Citations)
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
                foreach (var pub in tool.PublicationAssociations)
                {
                    if (!citations.ContainsKey(tool.ID))
                        citations.Add(tool.ID, new double[2]);

                    if (pub.Publication.Citations != null)
                        foreach (var citation in pub.Publication.Citations)
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
            // 1st int is tool ID, 1st double is days, and 2nd double is citation count.
            var changes = new Dictionary<int, SortedDictionary<double, double>>();
            foreach (var association in repository.ToolAssociations)
            {
                var tool = association.Tool;
                Context.Entry(tool).Collection(x => x.PublicationAssociations).Load();
                foreach (var pub in tool.PublicationAssociations)
                    if (pub.Publication.Citations != null && pub.Publication.Year >= EarliestPublicationYear)
                    {
                        if (pub.Publication.Citations.Count == 0 ||
                            (pub.Publication.Citations.Count == 1 && pub.Publication.Citations.First().Count == 0))
                            continue;

                        foreach (var citation in pub.Publication.Citations)
                        {
                            if (!changes.ContainsKey(tool.ID))
                                changes.Add(tool.ID, new SortedDictionary<double, double>());

                            var daysOffset = (citation.Date - association.DateAddedToRepository).Value.Days;
                            // This can be true when multiple publications per tool exist.
                            if (changes[tool.ID].ContainsKey(daysOffset))
                                changes[tool.ID][daysOffset] += citation.Count;
                            else
                                changes[tool.ID].Add(daysOffset,
                                    citation.Count);
                        }
                    }
            }

            // Normalize citation counts and date.
            var tools = changes.Keys.ToList();
            foreach (var toolID in tools)
            {
                var citations = changes[toolID];
                double minBeforeDays = 0;
                double maxAfterDays = 0;
                foreach (var day in citations.Keys)
                {
                    minBeforeDays = Math.Min(minBeforeDays, day);
                    maxAfterDays = Math.Max(maxAfterDays, day);
                }

                // Normalize citation count.
                var delta = maxAfterDays - minBeforeDays;
                var days = citations.Keys.ToList();
                foreach (var day in days)
                    citations[day] /= delta;

                // Min-Max normalize date.
                var normalizedDates = new SortedDictionary<double, double>();
                foreach (var day in days)
                {
                    var count = citations[day];
                    var normalizedDate = 0.0;
                    if (day <= 0)
                        normalizedDate = (-1) * ((day - maxAfterDays) / (minBeforeDays - maxAfterDays));
                    else
                        normalizedDate = (day - minBeforeDays) / (maxAfterDays - minBeforeDays);

                    normalizedDates.Add(normalizedDate, count);
                }

                changes[toolID] = normalizedDates;
            }

            var interpolatedCitations = new SortedDictionary<double, List<double>>();

            // Calculate in-betweens
            // First determine data points on x axis (i.e., days offset):
            var x = Generate.LinearSpaced(21, -1.0, 1.0);
            foreach (var item in x)
                interpolatedCitations.Add(item, new List<double>());

            foreach (var tool in changes)
            {
                if (tool.Value.Keys.Count < 2 || tool.Value.Values.Count < 2)
                    continue;
                // data samples to interpolate over
                var spline = Interpolate.Linear(tool.Value.Keys, tool.Value.Values);

                foreach (var item in x)
                {
                    var c = spline.Interpolate(item);
                    if (c < 0)
                    {
                        // todo: find a better alternative.
                        continue;
                    }
                    interpolatedCitations[item].Add(c);
                }
            }

            var rtv = new List<CitationChange>();
            foreach (var item in interpolatedCitations)
            {
                var c = new CitationChange();
                c.AddCitationCount(item.Value);
                c.DaysOffset = item.Key;
                // c.RemoveOutliers();
                rtv.Add(c);
            }

            return rtv;
        }

        public Dictionary<Tool, CitationChange> GetPrePostCitationChangeVector(
            IEnumerable<ToolRepoAssociation> associations)
        {
            Contract.Requires(associations != null);

            var rtv = new Dictionary<Tool, CitationChange>();

            // The points for in-betweens calculation; data points on x axis (i.e., days offset).
            var interpolationPoints = Generate.LinearSpaced(21, -1.0, 1.0);

            foreach (var asso in associations)
            {
                // Some tools may not have any publications. 
                if (asso.Tool.PublicationAssociations == null ||
                    asso.Tool.PublicationAssociations.Count == 0)
                    continue;

                if (!rtv.ContainsKey(asso.Tool))
                    rtv.Add(asso.Tool, new CitationChange());

                // There are some tools that have multiple publications, we consider only the first one. 
                var pub = asso.Tool.PublicationAssociations.First();

                if (pub.Publication.Citations != null &&
                    pub.Publication.Year >= EarliestPublicationYear)
                {
                    if (pub.Publication.Citations.Count == 0 ||
                        (pub.Publication.Citations.Count == 1 && pub.Publication.Citations.First().Count == 0) ||
                        pub.Publication.Citations.Count < 2) // At least two items are required for interpolation.
                        continue;

                    rtv[asso.Tool].AddRange(
                        pub.Publication.Citations,
                        asso.DateAddedToRepository,
                        interpolationPoints);
                }
            }

            return rtv;
        }

        public void GetPrePostCitationChangeVectorByPubs(
            IEnumerable<ToolRepoAssociation> associations,
            out Dictionary<int, CitationChange> vectors,
            out Dictionary<int, List<Tool>> tools)
        {
            Contract.Requires(associations != null);

            vectors = new Dictionary<int, CitationChange>();
            tools = new Dictionary<int, List<Tool>>();

            // The points for in-betweens calculation; data points on x axis (i.e., days offset).
            var interpolationPoints = Generate.LinearSpaced(21, -1.0, 1.0);

            foreach (var asso in associations)
            {
                // Some tools may not have any publications. 
                if (asso.Tool.PublicationAssociations == null ||
                    asso.Tool.PublicationAssociations.Count == 0 ||
                    asso.DateAddedToRepository == null)
                    continue;

                foreach (var pubAsso in asso.Tool.PublicationAssociations)
                {
                    if (pubAsso.Publication.Citations != null &&
                        pubAsso.Publication.Year >= EarliestPublicationYear &&
                        Math.Abs((int)pubAsso.Publication.Year - asso.DateAddedToRepository.Value.Year) >= MinPubDateAndDateAddedToRepoOffset)
                    {
                        if (pubAsso.Publication.Citations.Count == 0 ||
                            (pubAsso.Publication.Citations.Count == 1 && pubAsso.Publication.Citations.First().Count == 0) ||
                            pubAsso.Publication.Citations.Count < 2) // At least two items are required for interpolation.
                            continue;

                        var key = pubAsso.Publication.ID;
                        if (vectors.ContainsKey(key))
                        {
                            tools[key].Add(asso.Tool);
                            continue;
                        }

                        vectors.Add(key, new CitationChange());
                        tools.Add(key, new List<Tool> { asso.Tool });

                        vectors[key].AddRange(
                            pubAsso.Publication.Citations,
                            asso.DateAddedToRepository,
                            interpolationPoints);
                    }
                }
            }
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
