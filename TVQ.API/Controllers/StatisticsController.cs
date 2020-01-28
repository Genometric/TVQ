using Genometric.TVQ.API.Analysis;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly TVQContext _context;

        private enum ReportTypes
        {
            BeforeAfterCitationCountPerTool,
            BeforeAfterCitationCountPerToolNormalizedPerYear,
            CreateTimeDistributionPerYear,
            CreateTimeDistributionPerMonth
        };

        public StatisticsController(TVQContext context)
        {
            _context = context;
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

        // GET: api/v1/Statistics/5/Downloads
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
                case ReportTypes.CreateTimeDistributionPerYear:
                    return CreateTimeDistributionPerYear(repository);
                case ReportTypes.CreateTimeDistributionPerMonth:
                    return CreateTimeDistributionPerMonth(repository);
            }

            return BadRequest();
        }

        private FileStreamResult BeforeAfterCitationCountPerTool(Repository repository)
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

            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            foreach (var item in citations)
                writer.WriteLine($"{item.Value[0]}\t{item.Value[1]}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }

        private FileStreamResult BeforeAfterCitationCountPerToolNormalizedPerYear(Repository repository)
        {
            var changes = AnalysisService.GetPrePostCitationCountNormalizedYear(repository);
            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            foreach (var tool in changes)
                foreach (var change in tool.Value)
                    writer.WriteLine($"{tool.Key}\t{change.DaysOffset}\t{change.Count}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return File(stream, contentType, fileName);
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

            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            foreach (var d in dist)
                writer.WriteLine($"{d.Key}\t{d.Value}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, System.IO.SeekOrigin.Begin);
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

            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            foreach (var d in dist)
                writer.WriteLine($"{d.Key}\t{d.Value}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return File(stream, contentType, fileName);
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
                            .ThenInclude(x => x.Publications)
                                .ThenInclude(x => x.Citations)
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
                            .ThenInclude(x => x.Publications)
                    .First(x => x.ID == id);
            }
        }
    }
}
