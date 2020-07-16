using Genometric.TVQ.WebService.Infrastructure;
using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks;
using Genometric.TVQ.WebService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class RepoCrawlingJobsController : ControllerBase
    {
        private readonly TVQContext _context;
        private readonly IBaseBackgroundTaskQueue<RepoCrawlingJob> _queue;
        private readonly ILogger<RepoCrawlingJobsController> _logger;

        public RepoCrawlingJobsController(
            TVQContext context,
            IBaseBackgroundTaskQueue<RepoCrawlingJob> queue,
            ILogger<RepoCrawlingJobsController> logger)
        {
            _context = context;
            _queue = queue;
            _logger = logger;
        }

        // GET: api/v1/RepoCrawlingJobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepoCrawlingJob>>> GetRepoCrawlingJobs()
        {
            return await _context.RepoCrawlingJobs.Include(x => x.Repository)
                                                  .ToListAsync()
                                                  .ConfigureAwait(false);
        }

        // GET: api/v1/RepoCrawlingJobs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RepoCrawlingJob>> GetRepoCrawlingJob(int id)
        {
            var repoCrawlingJob = await _context.RepoCrawlingJobs.FindAsync(id);
            if (repoCrawlingJob == null)
                return NotFound();
            return repoCrawlingJob;
        }

        // POST: api/v1/RepoCrawlingJobs
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        /// <summary>
        /// Example: 
        /// {
        ///     "Repository":
        ///     {
        ///         "id" : "3"
        ///     }
        /// }
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<RepoCrawlingJob>> PostRepoCrawlingJob(RepoCrawlingJob job)
        {
            if (job == null)
                return BadRequest();
            if (job.Repository == null)
                return BadRequest("missing repository ID.");
            
            var repository = _context.Repositories.Find(job.Repository.ID);
            if (repository == null)
                return BadRequest("invalid repository ID.");

            if (_context.RepoCrawlingJobs.Any(
                x => (x.Status == State.Queued || x.Status == State.Running)
                     && x.Repository.ID == repository.ID))
                return BadRequest($"The repository {repository.ID} is already set to be crawled.");

            job.Repository = repository;
            job.Status = default;

            _context.RepoCrawlingJobs.Add(job);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            _queue.Enqueue(job.ID);

            return CreatedAtAction("GetRepoCrawlingJob", new { id = job.ID }, job);
        }

        // DELETE: api/v1/RepoCrawlingJobs/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<RepoCrawlingJob>> DeleteRepoCrawlingJob(int id)
        {
            var repoCrawlingJob = await _context.RepoCrawlingJobs.FindAsync(id);
            if (repoCrawlingJob == null)
            {
                return NotFound();
            }

            _context.RepoCrawlingJobs.Remove(repoCrawlingJob);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return repoCrawlingJob;
        }

        private bool RepoCrawlingJobExists(int id)
        {
            return _context.RepoCrawlingJobs.Any(e => e.ID == id);
        }
    }
}
