﻿using Genometric.TVQ.API.Analysis;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Genometric.TVQ.API.Model.Associations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        // ----------------------------------------------------
        // All the methods of this controller are experimental. 
        // ----------------------------------------------------

        private readonly TVQContext _context;
        private readonly AnalysisService _analysisService;

        private const string _numberFormat = "0.00000";

        private enum ReportTypes
        {
            BeforeAfterCitationCountPerTool,
            BeforeAfterCitationCountPerToolNormalizedPerYear,
            BeforeAfterCitationCountPerToolNormalizedPerYearPerCategory,
            CreateTimeDistributionPerYear,
            CreateTimeDistributionPerMonth,
            ToolDistributionAmongRepositories,
            ToolDistributionAmongRepositoriesExpression,
            ToolDistributionAmongCategories,
            ToolDistributionAmongCategoriesPerYear,
            NormalizedBeforeAfterVector,
            BetweenRepoTTest,
            DownloadFeatures,
            Overview
        };

        public StatisticsController(
            TVQContext context,
            AnalysisService analysisService)
        {
            _context = context;
            _analysisService = analysisService;
        }

        // GET: api/v1/Statistics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Statistics>>> GetStatistics()
        {
            return await _context.Statistics.ToListAsync()
                                            .ConfigureAwait(false);
        }

        // GET: api/v1/Statistics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Statistics>> GetStatistics(int id)
        {
            var statistics = await _context.Statistics.FindAsync(id);
            if (statistics == null)
                return NotFound();

            return statistics;
        }

        // GET: api/v1/Statistics/1/downloads?type=BeforeAfterCitationCountPerTool
        [HttpGet("{id}/downloads")]
        public async Task<IActionResult> Downloads([FromRoute] int id, [FromQuery] string type)
        {
            if (!Enum.TryParse(type, ignoreCase: true, out ReportTypes reportType))
            {
                var builder = new StringBuilder("The required report type is missing from " +
                                                "the query (e.g., ?type=X); supported values for type are: ");
                foreach (var t in (ReportTypes[])Enum.GetValues(typeof(ReportTypes)))
                    builder.Append(t.ToString());
                return BadRequest(builder.ToString());
            }

            var statistics = await _context.Statistics.FindAsync(id);
            if (statistics == null)
                return NotFound();

            var repository = QueryRepo(statistics.RepositoryID, true);
            if (repository == null)
                return NotFound();

            switch (reportType)
            {
                case ReportTypes.BeforeAfterCitationCountPerTool:
                    return BeforeAfterCitationCountPerTool(repository);
                case ReportTypes.BeforeAfterCitationCountPerToolNormalizedPerYear:
                    return BeforeAfterCitationCountPerToolNormalizedPerYear(repository);
                case ReportTypes.BeforeAfterCitationCountPerToolNormalizedPerYearPerCategory:
                    return BeforeAfterCitationCountPerToolNormalizedPerYearPerCategory(repository);
                case ReportTypes.CreateTimeDistributionPerYear:
                    return CreateTimeDistributionPerYear(repository);
                case ReportTypes.CreateTimeDistributionPerMonth:
                    return CreateTimeDistributionPerMonth(repository);
                case ReportTypes.ToolDistributionAmongRepositories:
                    return Ok(await ToolDistributionAmongRepositories().ConfigureAwait(false));
                case ReportTypes.ToolDistributionAmongRepositoriesExpression:
                    return Ok(await ToolDistributionAmongRepositoriesExpression().ConfigureAwait(false));
                case ReportTypes.ToolDistributionAmongCategories:
                    return Ok(await ToolDistributionAmongCategories().ConfigureAwait(false));
                case ReportTypes.ToolDistributionAmongCategoriesPerYear:
                    return await ToolDistributionAmongCategoriesPerYear(repository).ConfigureAwait(false);
                case ReportTypes.NormalizedBeforeAfterVector:
                    return NormalizedBeforeAfterVector(repository);
                case ReportTypes.Overview:
                    return await Overview(repository).ConfigureAwait(false);
                case ReportTypes.BetweenRepoTTest:
                    return Ok(await BetweenRepoTTest().ConfigureAwait(false));
                case ReportTypes.DownloadFeatures:
                    return DownloadFeatures();
            }

            return BadRequest();
        }

        private FileStreamResult BeforeAfterCitationCountPerTool(Repository repository)
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

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            foreach (var item in citations)
                writer.WriteLine($"{item.Value[0]}\t{item.Value[1]}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }

        private FileStreamResult BeforeAfterCitationCountPerToolNormalizedPerYear(Repository repository)
        {
            var changes = _analysisService.GetPrePostCitationCountNormalizedYear(repository);
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            foreach (var tool in changes)
                foreach (var change in tool.Value)
                    writer.WriteLine($"All_Categories\t{tool.Key}\t{change.DaysOffset}\t{change.CitationCount}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }

        private FileStreamResult BeforeAfterCitationCountPerToolNormalizedPerYearPerCategory(Repository repository)
        {
            var tempPath =
                Path.GetFullPath(Path.GetTempPath()) +
                Utilities.GetRandomString(10) +
                Path.DirectorySeparatorChar;

            Directory.CreateDirectory(tempPath);

            var fileNames = new List<string>();

            // This method is certainly very sub-optimal; it should be re-implemented. 
            foreach (var category in _context.Categories.ToList())
            {
                var tools = new List<int>();
                // This is a very slow query with multiple joins, should be improved.
                var toolRepoAssociations =
                    _context.ToolRepoAssociations.Where(x => x.RepositoryID == repository.ID)
                                                .Include(x => x.Tool)
                                                .ThenInclude(x => x.CategoryAssociations)
                                                .ThenInclude(x => x.Category)
                                                .ToList();

                foreach (var x in toolRepoAssociations)
                    foreach (var y in x.Tool.CategoryAssociations)
                        if (y.Category.ID == category.ID)
                        {
                            tools.Add(x.Tool.ID);
                            break;
                        }

                var changes = _analysisService.GetPrePostCitationCountNormalizedYear(repository, new HashSet<int>(tools));

                var filename = tempPath + Utilities.SafeFilename(category.Name + ".csv");
                using (var writer = new StreamWriter(filename))
                    foreach (var tool in changes)
                        foreach (var change in tool.Value)
                            writer.WriteLine($"{category.Name}\t{tool.Key}\t{change.DaysOffset}\t{change.CitationCount}");
            }

            var zipFileTempPath = Path.GetFullPath(Path.GetTempPath()) + Utilities.GetRandomString(10) + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(zipFileTempPath);
            var zipFilename = $"TVQStats.zip";
            ZipFile.CreateFromDirectory(tempPath, zipFileTempPath + zipFilename);

            var contentType = "application/zip";
            IFileProvider provider = new PhysicalFileProvider(zipFileTempPath);
            IFileInfo fileInfo = provider.GetFileInfo(zipFilename);

            return File(fileInfo.CreateReadStream(), contentType, zipFilename);
        }

        private FileStreamResult CreateTimeDistributionPerYear(Repository repository)
        {
            var dist = new Dictionary<int, int>();
            foreach (var association in repository.ToolAssociations)
            {
                var year = ((DateTime)association.DateAddedToRepository).Year;
                if (dist.ContainsKey(year))
                    dist[year]++;
                else
                    dist.Add(year, 1);
            }

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            foreach (var d in dist)
                writer.WriteLine($"{d.Key}\t{d.Value}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }

        private FileStreamResult CreateTimeDistributionPerMonth(Repository repository)
        {
            var dist = new Dictionary<string, int>();
            foreach (var association in repository.ToolAssociations)
            {
                var date = ((DateTime)association.DateAddedToRepository);
                var key = $"{date.Year}-{date.Month}";
                if (dist.ContainsKey(key))
                    dist[key]++;
                else
                    dist.Add(key, 1);
            }

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            foreach (var d in dist)
                writer.WriteLine($"{d.Key}\t{d.Value}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }

        private async Task<IEnumerable<ToolRepoDistribution>> ToolDistributionAmongRepositories()
        {
            var distributions = new Dictionary<string, ToolRepoDistribution>();
            var tools = await _context.Tools.Include(x => x.RepoAssociations)
                                            .ThenInclude(x => x.Repository)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

            static string HashID(ToolRepoAssociation association) =>
                association.RepositoryID.ToString(CultureInfo.InvariantCulture) + ";";

            static string HashIDs(IEnumerable<ToolRepoAssociation> associations)
            {
                var builder = new StringBuilder();
                foreach (var association in associations)
                    builder.Append(HashID(association));
                return builder.ToString();
            }

            void AddOrUpdate(IEnumerable<ToolRepoAssociation> associations)
            {
                if (distributions.TryGetValue(HashIDs(associations), out ToolRepoDistribution dist))
                    dist.Count++;
                else
                {
                    var item = new ToolRepoDistribution();
                    item.Add(associations.Select(x => x.Repository));
                    item.Count = 1;
                    distributions.Add(HashIDs(associations), item);
                }
            }

            foreach (var tool in tools)
            {
                foreach (var association in tool.RepoAssociations)
                    AddOrUpdate(new ToolRepoAssociation[] { association });

                if (tool.RepoAssociations.Count > 1)
                    AddOrUpdate(tool.RepoAssociations);
            }

            foreach (var dist in distributions)
                dist.Value.Percentage = dist.Value.Count / (double)tools.Count;

            return distributions.Values;
        }

        // This method generates an output that can be as input to the UpSet plot shiny app (https://gehlenborglab.shinyapps.io/upsetr/). 
        private async Task<string> ToolDistributionAmongRepositoriesExpression()
        {
            var toolRepoDis = await ToolDistributionAmongRepositories().ConfigureAwait(false);
            var builder = new StringBuilder();

            foreach (var intersection in toolRepoDis)
            {
                builder.Append(string.Join("&", intersection.Repositories.Select(x => x.Name)));
                builder.Append($"={intersection.Count},");
            }

            return builder.ToString();
        }

        private async Task<IEnumerable<ToolCategoryDistribution>> ToolDistributionAmongCategories()
        {
            var distributions = new Dictionary<string, ToolCategoryDistribution>();
            var tools = await _context.Tools.Include(x => x.CategoryAssociations)
                                            .ThenInclude(x => x.Category)
                                            .ToListAsync()
                                            .ConfigureAwait(false);

            static string HashID(ToolCategoryAssociation association) =>
                association.CategoryID.ToString(CultureInfo.InvariantCulture) + ";";

            static string HashIDs(IEnumerable<ToolCategoryAssociation> associations)
            {
                var builder = new StringBuilder();
                foreach (var association in associations)
                    builder.Append(HashID(association));
                return builder.ToString();
            }

            void AddOrUpdate(IEnumerable<ToolCategoryAssociation> associations)
            {
                if (distributions.TryGetValue(HashIDs(associations), out ToolCategoryDistribution dist))
                    dist.Count++;
                else
                {
                    var item = new ToolCategoryDistribution();
                    item.Add(associations.Select(x => x.Category));
                    item.Count = 1;
                    distributions.Add(HashIDs(associations), item);
                }
            }

            foreach (var tool in tools)
            {
                foreach (var association in tool.CategoryAssociations)
                    AddOrUpdate(new ToolCategoryAssociation[] { association });

                if (tool.RepoAssociations.Count > 1)
                    AddOrUpdate(tool.CategoryAssociations);
            }

            foreach (var dist in distributions)
                dist.Value.Percentage = dist.Value.Count / (double)tools.Count;

            return distributions.Values;
        }

        private async Task<IActionResult> ToolDistributionAmongCategoriesPerYear(Repository repository)
        {
            var distributions = new SortedDictionary<int, SortedDictionary<string, double>>();

            var associations = await _context.ToolRepoAssociations.Include(x => x.Tool)
                                                                  .ThenInclude(x => x.CategoryAssociations)
                                                                  .ThenInclude(x => x.Category)
                                                                  .Where(x => x.RepositoryID == repository.ID)
                                                                  .ToListAsync()
                                                                  .ConfigureAwait(false);

            var repoID = repository.ID;
            if(repository.Name == Repository.Repo.Bioconda)
            {
                var repos = _context.Repositories.ToList();
                repoID = repos.Find(x => x.Name == Repository.Repo.BioTools).ID;
            }

            var categoriesInRepo = await _context.CategoryRepoAssociations.Include(x => x.Category)
                                                                          .Include(x => x.Repository)
                                                                          .Where(x => x.RepositoryID == repoID)
                                                                          .ToDictionaryAsync(x => x.CategoryID, x => x.Category)
                                                                          .ConfigureAwait(false);

            string unspecifiedCategory = "Unspecified";
            var categoriesName = new SortedSet<string>
            {
                unspecifiedCategory
            };

            foreach (var association in associations)
            {
                var c = false;
                foreach(var categoryAssociation in association.Tool.CategoryAssociations)
                    if (categoriesInRepo.ContainsKey(categoryAssociation.CategoryID))
                    {
                        c = true;
                        break;
                    }

                if (!c) continue;

                var date = association.DateAddedToRepository.Value.Year;
                if (!distributions.ContainsKey(date))
                    distributions.Add(date, new SortedDictionary<string, double>());

                if (association.Tool.CategoryAssociations.Count == 0)
                {
                    if (!distributions[date].ContainsKey(unspecifiedCategory))
                        distributions[date].Add(unspecifiedCategory, 0.0);
                    distributions[date][unspecifiedCategory]++;
                }
                else
                {
                    foreach (var categoryAssociation in association.Tool.CategoryAssociations)
                    {
                        if (!categoriesInRepo.ContainsKey(categoryAssociation.CategoryID))
                            continue;

                        var categoryName = categoryAssociation.Category.Name;
                        if (categoryName == null) 
                            continue;
                        if (!distributions[date].ContainsKey(categoryName))
                            distributions[date].Add(categoryName, 0.0);

                        distributions[date][categoryName]++;

                        if (!categoriesName.Contains(categoryName))
                            categoriesName.Add(categoryName);
                    }
                }
            }

            var dates = distributions.Keys.ToList();
            for (int i = 1; i < dates.Count; i++)
            {
                foreach (var category in categoriesName)
                {
                    var accumulatedToDate = 0.0;
                    if (distributions[dates[i - 1]].ContainsKey(category))
                        accumulatedToDate = distributions[dates[i - 1]][category];

                    if (!distributions[dates[i]].ContainsKey(category))
                        distributions[dates[i]].Add(category, accumulatedToDate);
                    else
                        distributions[dates[i]][category] += accumulatedToDate;
                }
            }

            var tempPath = Path.GetFullPath(Path.GetTempPath()) + Utilities.GetRandomString(10) + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(tempPath);
            var filename = tempPath + Utilities.SafeFilename("TVQStats.csv");

            using (var writer = new StreamWriter(filename))
            {
                var headerBuilder = new StringBuilder();
                headerBuilder.Append("Category\t");
                foreach (var year in distributions)
                    headerBuilder.Append(year.Key + "\t");
                writer.WriteLine(headerBuilder.ToString());

                foreach (var name in categoriesName)
                {
                    var builder = new StringBuilder();
                    builder.Append(name + "\t");

                    foreach (var year in distributions)
                        if (year.Value.ContainsKey(name))
                            builder.Append(year.Value[name] + "\t");
                        else
                            builder.Append("0\t");

                    writer.WriteLine(builder.ToString());
                }
            }

            var contentType = "application/csv";
            IFileProvider provider = new PhysicalFileProvider(tempPath);
            IFileInfo fileInfo = provider.GetFileInfo("TVQStats.csv");

            return File(fileInfo.CreateReadStream(), contentType, "TVQStats.csv");
        }

        private async Task<IEnumerable<ToolRepoDistribution>> BetweenRepoTTest()
        {
            var rtv = new List<ToolRepoDistribution>();
            var repos = _context.Repositories.Include(repo => repo.ToolAssociations)
                                                .ThenInclude(x => x.Tool)
                                             .Include(repo => repo.ToolAssociations)
                                                .ThenInclude(x => x.Downloads)
                                             .Include(repo => repo.ToolAssociations)
                                                .ThenInclude(x => x.Tool)
                                                    .ThenInclude(x => x.PublicationAssociations)
                                                        .ThenInclude(x => x.Publication.Citations)
                                             .Include(repo => repo.Statistics)
                                             .ToList();

            for (int i = 0; i < repos.Count - 1; i++)
            {
                for (int j = i + 1; j < repos.Count; j++)
                {
                    var repoADeltas = _analysisService.GetDeltaPrePostCitationChanges(repos[i]);
                    var repoBDeltas = _analysisService.GetDeltaPrePostCitationChanges(repos[j]);

                    var sigDiff = InferentialStatistics.ComputeTTest(repoADeltas,
                                                                     repoBDeltas,
                                                                     0.05,
                                                                     out double df,
                                                                     out double tScore,
                                                                     out double pValue,
                                                                     out double criticalValue,
                                                                     doubleSide: false);
                    var toolRepoDist = new ToolRepoDistribution();
                    toolRepoDist.Add(repos[i]);
                    toolRepoDist.Add(repos[j]);
                    toolRepoDist.Statistics = new Statistics()
                    {
                        TScore = tScore,
                        PValue = pValue,
                        DegreeOfFreedom = df,
                        CriticalValue = criticalValue,
                        MeansSignificantlyDifferent = sigDiff
                    };

                    rtv.Add(toolRepoDist);
                }
            }

            return rtv;
        }

        private FileStreamResult NormalizedBeforeAfterVector(Repository repository)
        {
            var tempPath = Path.GetFullPath(Path.GetTempPath()) + Utilities.GetRandomString(10) + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(tempPath);
            var filename = tempPath + Utilities.SafeFilename("TVQStats.csv");

            var changes = _analysisService.GetPrePostCitationChangeVector(repository);
            using (var writer = new StreamWriter(filename))
            {
                foreach (var change in changes)
                    writer.WriteLine(
                        $"All_Categories\t" +
                        $"{change.DaysOffset}\t" +
                        $"{change.LowerQuartile}\t" +
                        $"{change.Median}\t" +
                        $"{change.UpperQuartile}\t" +
                        $"{change.Max}\t" +
                        $"{change.Min}");
            }

            var contentType = "application/csv";
            IFileProvider provider = new PhysicalFileProvider(tempPath);
            IFileInfo fileInfo = provider.GetFileInfo("TVQStats.csv");

            return File(fileInfo.CreateReadStream(), contentType, "TVQStats.csv");
        }

        private FileStreamResult DownloadFeatures()
        {
            var tempPath =
                Path.GetFullPath(Path.GetTempPath()) +
                Utilities.GetRandomString(10) +
                Path.DirectorySeparatorChar;

            Directory.CreateDirectory(tempPath);

            var fileNames = new List<string>();

            var toolRepoAssociations = _context.ToolRepoAssociations
                .Include(x => x.Tool).ThenInclude(x => x.CategoryAssociations)
                .Include(x => x.Tool).ThenInclude(x => x.CategoryAssociations)
                .Include(x => x.Tool).ThenInclude(x => x.PublicationAssociations).ThenInclude(x => x.Publication)
                .ToList();

            foreach(var repo in _context.Repositories)
            {
                var associations = toolRepoAssociations.Where(x => x.RepositoryID == repo.ID).ToList();

                var normalizedData = _analysisService.GetPrePostCitationChangeVector(associations);

                if (normalizedData.Count == 0)
                    continue;

                var builder = new StringBuilder();
                var filename = tempPath + Utilities.SafeFilename(repo.Name + ".csv");
                using var writer = new StreamWriter(filename);

                builder.Append("ID\tToolName");
                var pointsX = normalizedData.First().Value.CitationsVector.Keys;
                foreach (var x in pointsX)
                    builder.Append("\t" + x.ToString(_numberFormat, CultureInfo.InvariantCulture));
                writer.WriteLine(builder.ToString());
                
                foreach (var tool in normalizedData)
                {
                    if (tool.Value.CitationsVector.Count == 0)
                        continue;

                    builder.Clear();
                    builder.Append(tool.Key.ToString());

                    foreach (var point in tool.Value.CitationsVector)
                        builder.Append("\t" + point.Value.ToString(_numberFormat, CultureInfo.InvariantCulture));

                    writer.WriteLine(builder.ToString());
                }
            }

            var zipFileTempPath = Path.GetFullPath(Path.GetTempPath()) + Utilities.GetRandomString(10) + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(zipFileTempPath);
            var zipFilename = $"TVQStats.zip";
            ZipFile.CreateFromDirectory(tempPath, zipFileTempPath + zipFilename);

            var contentType = "application/zip";
            IFileProvider provider = new PhysicalFileProvider(zipFileTempPath);
            IFileInfo fileInfo = provider.GetFileInfo(zipFilename);

            return File(fileInfo.CreateReadStream(), contentType, zipFilename);
        }

        private async Task<IActionResult> Overview(Repository repository)
        {
            var overview = new Overview();

            overview.RepositoryCount = _context.Repositories.Count();
            overview.ToolsCountInAllRepositories = _context.Tools.Count();

            // Do NOT uncomment the commented-out methods as they may
            // cause such a big join (on BioTools in particular) that
            // will throw a database connection timeout.
            var repo = _context.Repositories//.Include(x => x.ToolAssociations)
                                            //.ThenInclude(x => x.Tool)
                                            //.ThenInclude(x => x.PublicationAssociations)
                                            .Include(x => x.CategoryAssociations)
                                            .First(x => x.ID == repository.ID);

            overview.ToolRepoAssociationsCount = repo.ToolAssociations.Count;

            foreach (var association in repo.ToolAssociations)
            {
                switch (association.Tool.PublicationAssociations.Count)
                {
                    case 0:
                        overview.ToolsWithNoPublications += 1;
                        break;

                    case 1:
                        overview.ToolsWithOnePublication += 1;
                        break;

                    default:
                        overview.ToolsWithMoreThanOnePublications += 1;
                        break;
                }
            }

            overview.CategoryAssociationsCount = repo.CategoryAssociations.Count;

            return Ok(overview);
        }

        private Repository QueryRepo(int id, bool includeCitations = false)
        {
            if (includeCitations)
            {
                return
                    _context.Repositories
                    .Include(repo => repo.ToolAssociations)
                        .ThenInclude(x => x.Tool)
                    .Include(repo => repo.ToolAssociations)
                        .ThenInclude(x => x.Downloads)
                    .Include(repo => repo.ToolAssociations)
                        .ThenInclude(x => x.Tool)
                            .ThenInclude(x => x.PublicationAssociations)
                                .ThenInclude(x => x.Publication.Citations)
                    .Include(repo => repo.Statistics)
                    .First(x => x.ID == id);
            }
            else
            {
                return
                    _context.Repositories
                    .Include(repo => repo.ToolAssociations)
                        .ThenInclude(x => x.Tool)
                    .Include(repo => repo.ToolAssociations)
                        .ThenInclude(x => x.Downloads)
                    .Include(repo => repo.ToolAssociations)
                        .ThenInclude(x => x.Tool)
                            .ThenInclude(x => x.PublicationAssociations)
                    .First(x => x.ID == id);
            }
        }
    }
}
