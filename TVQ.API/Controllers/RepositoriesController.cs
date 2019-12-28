using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IBackgroundToolRepoCrawlingQueue _queue;
        private readonly IBackgroundAnalysisTaskQueue _analysisQueue;
        private readonly ILogger<RepositoriesController> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;

        public RepositoriesController(
            TVQContext context,
            IBackgroundToolRepoCrawlingQueue queue,
            IBackgroundAnalysisTaskQueue analysisQueue,
            IBackgroundTaskQueue taskQueue,
            ILogger<RepositoriesController> logger)
        {
            _context = context;
            _queue = queue;
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
                .Include(x => x.Tools)
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
        public async Task<IActionResult> PutRepo([FromRoute] int id, [FromBody] Repository dataItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dataItem.ID)
                return BadRequest();

            _context.Entry(dataItem).State = EntityState.Modified;

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

            return NoContent();
        }

        // POST: api/v1/repositories
        [HttpPost]
        public async Task<IActionResult> PostRepo([FromBody] Repository dataItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Repositories.Add(dataItem);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return CreatedAtAction("GetRequestItems", new { }, dataItem);
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

        // GET: api/v1/repositories/scan/1
        [HttpGet("{id}/scan")]
        public async Task<IActionResult> ScanToolsInRepo([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = await 
                _context.Repositories
                .Include(repo => repo.Tools)
                    .ThenInclude(tool => tool.Downloads)
                .Include(repo => repo.Tools)
                    .ThenInclude(tool => tool.Publications)
                .FirstAsync(x=>x.ID == id)
                .ConfigureAwait(false);

            if (repository == null)
                return NotFound();

            _queue.QueueBackgroundWorkItem(repository);

            return Ok(repository);
        }

        [HttpGet("{id}/analysis")]
        public async Task<IActionResult> RunAnalysis([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = await
                _context.Repositories
                .Include(repo => repo.Tools)
                    .ThenInclude(tool => tool.Downloads)
                .Include(repo => repo.Tools)
                    .ThenInclude(tool => tool.Publications)
                        .ThenInclude(x => x.Citations)
                .Include(repo => repo.Statistics)
                .FirstAsync(x => x.ID == id)
                .ConfigureAwait(false);

            if (repository == null)
                return NotFound();

            _analysisQueue.QueueBackgroundWorkItem(repository);

            return Ok(repository);
        }

        // THIS IS ENDPOINT IS FOR TESTING PURPOSES.
        [HttpGet("{id}/downloadstats")]
        public async Task<IActionResult> DownloadStats([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = await
                _context.Repositories
                .Include(repo => repo.Tools)
                    .ThenInclude(tool => tool.Downloads)
                .Include(repo => repo.Tools)
                    .ThenInclude(tool => tool.Publications)
                        .ThenInclude(x => x.Citations)
                .Include(repo => repo.Statistics)
                .FirstAsync(x => x.ID == id)
                .ConfigureAwait(false);

            if (repository == null)
                return NotFound();

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

            var stream = new System.IO.MemoryStream();
            var writer = new System.IO.StreamWriter(stream);
            foreach (var item in citations)
                writer.WriteLine($"{item.Value[0]}\t{item.Value[1]}");

            var contentType = "APPLICATION/octet-stream";
            var fileName = "TVQStats.csv";
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }

        private bool RepoExists(int id)
        {
            return _context.Repositories.Any(e => e.ID == id);
        }
    }
}