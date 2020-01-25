using Genometric.TVQ.API.Analysis;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RepositoriesController : ControllerBase
    {
        private readonly TVQContext _context;
        private readonly IBackgroundAnalysisTaskQueue _analysisQueue;
        private readonly ILogger<RepositoriesController> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;

        public RepositoriesController(
            TVQContext context,
            IBackgroundAnalysisTaskQueue analysisQueue,
            IBackgroundTaskQueue taskQueue,
            ILogger<RepositoriesController> logger)
        {
            _context = context;
            _analysisQueue = analysisQueue;
            _logger = logger;
            _taskQueue = taskQueue;
        }

        // GET: api/v1/repositories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Repository>>> GetRepos()
        {
            return await
                _context.Repositories
                .Include(x => x.ToolAssociations)
                    .ThenInclude(x => x.Tool)
                .Include(x => x.Statistics)
                .ToListAsync().ConfigureAwait(false);
        }

        // GET: api/v1/repositories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRepo([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var DataItem = await _context.Repositories.FindAsync(id);
            if (DataItem == null)
                return NotFound();

            return Ok(DataItem);
        }

        // PUT: api/v1/repositories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRepo([FromRoute] int id, [FromBody] Repository repository)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != repository.ID)
                return BadRequest();

            _context.Entry(repository).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RepoExists(id))
                    return NotFound();
                else
                    throw;
            }

            return Ok(repository);
        }

        // POST: api/v1/repositories
        [HttpPost]
        public async Task<IActionResult> PostRepo([FromBody] Repository repository)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Repositories.Add(repository);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return CreatedAtAction("GetRequestItems", new { }, repository);
        }

        // DELETE: api/v1/repositories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRepo([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dataItem = await _context.Repositories.FindAsync(id);
            if (dataItem == null)
                return NotFound();

            _context.Repositories.Remove(dataItem);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(dataItem);
        }

        // THIS IS ENDPOINT IS FOR TESTING PURPOSES.
        [HttpGet("{id}/downloadstats")]
        public async Task<IActionResult> DownloadStats([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = QueryRepo(id, true);

            if (repository == null)
                return NotFound();

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

        [HttpGet("{id}/downloadstats2")]
        public async Task<IActionResult> DownloadStats2([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = QueryRepo(id, true);

            if (repository == null)
                return NotFound();

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

        // THIS IS AN EXPERIMENTAL ENDPOINT.
        [HttpGet("{id}/createtime_dist_year")]
        public async Task<IActionResult> CreateTimeDistYear([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = QueryRepo(id);
            var dist = new Dictionary<int, int>();
            foreach (var association in repository.ToolAssociations)
            {
                var year = ((DateTime)association.DateAddedToRepository).Year;
                if (dist.ContainsKey(year))
                    dist[year]++;
                else
                    dist.Add(year, 1);
            }

            var distributions = (from record in dist
                                 select new int[] { record.Key, record.Value }).ToArray();

            return Ok(distributions);
        }

        // THIS IS AN EXPERIMENTAL ENDPOINT.
        [HttpGet("{id}/createtime_dist_month")]
        public async Task<IActionResult> CreateTimeDistMonth([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = QueryRepo(id);
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

            var distributions = (from record in dist
                                 select new string[] { record.Key, record.Value.ToString() }).ToArray();

            return Ok(distributions);
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

        private bool RepoExists(int id)
        {
            return _context.Repositories.Any(e => e.ID == id);
        }
    }
}