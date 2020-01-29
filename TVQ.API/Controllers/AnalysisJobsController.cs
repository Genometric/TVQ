using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class AnalysisJobsController : ControllerBase
    {
        private readonly TVQContext _context;
        private IBaseBackgroundTaskQueue<AnalysisJob> AnalysisQueue { get; }

        public AnalysisJobsController(
            TVQContext context,
            IBaseBackgroundTaskQueue<AnalysisJob> analysisQueue)
        {
            _context = context;
            AnalysisQueue = analysisQueue;
        }

        // GET: api/v1/AnalysisJobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnalysisJob>>> GetAnalysisJobs()
        {
            return await _context.AnalysisJobs.ToListAsync().ConfigureAwait(false);
        }

        // GET: api/v1/AnalysisJobs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnalysisJob>> GetAnalysisJob(int id)
        {
            var analysisJob = await _context.AnalysisJobs.FindAsync(id);
            if (analysisJob == null)
                return NotFound();
            return analysisJob;
        }

        // POST: api/v1/AnalysisJobs
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<AnalysisJob>> PostAnalysisJob(AnalysisJob job)
        {
            if (job == null)
                return BadRequest();
            if (job.Repository == null)
                return BadRequest("missing repository ID.");

            var repository = _context.Repositories.Find(job.Repository.ID);
            if (repository == null)
                return BadRequest("invalid repository ID.");

            if (_context.RepoCrawlingJobs
                .Any(x => (x.Status == State.Queued || x.Status == State.Running) &&
                          x.Repository.ID == repository.ID))
                return BadRequest($"The repository {repository.ID} is already set to be crawled.");

            job.Repository = repository;
            job.Status = default;

            _context.AnalysisJobs.Add(job);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            AnalysisQueue.Enqueue(job.ID);
            return CreatedAtAction("GetAnalysisJob", new { id = job.ID }, job);
        }
    }
}
