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
    public class LiteratureCrawlingJobsController : ControllerBase
    {
        private readonly TVQContext _context;
        private IBaseBackgroundTaskQueue<LiteratureCrawlingJob> Queue { get; }

        public LiteratureCrawlingJobsController(
            TVQContext context,
            IBaseBackgroundTaskQueue<LiteratureCrawlingJob> queue)
        {
            _context = context;
            Queue = queue;
        }

        // GET: api/v1/LiteratureCrawlingJobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LiteratureCrawlingJob>>> GetLiteratureCrawlingJobs()
        {
            return await _context.LiteratureCrawlingJobs.ToListAsync().ConfigureAwait(false);
        }

        // GET: api/v1/LiteratureCrawlingJobs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LiteratureCrawlingJob>> GetLiteratureCrawlingJob(int id)
        {
            var literatureCrawlingJob = await _context.LiteratureCrawlingJobs.FindAsync(id);
            if (literatureCrawlingJob == null)
                return NotFound();
            return literatureCrawlingJob;
        }

        // POST: api/v1/LiteratureCrawlingJobs
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        /// <summary>
        /// Example:
        /// {
        ///     "publications":
        ///     [
        ///         {
        ///             "id": 3,
        ///         },
        ///         {
        ///             "id": 9
        ///         }
        ///     ]
        /// }
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<LiteratureCrawlingJob>> PostLiteratureCrawlingJob(LiteratureCrawlingJob job)
        {
            if (job == null)
                return BadRequest();

            if (job.ScanAllPublications == false &&
                job.Publications == null)
                return BadRequest(
                    "No publications to scan are provided; either set " +
                    "`ScanAllPublications = true` or provide a list " +
                    "of publications to scan.");

            if (job.ScanAllPublications == true)
            {
                if (_context.LiteratureCrawlingJobs.Any(
                    x => x.ScanAllPublications == true &&
                         (x.Status == State.Queued || x.Status == State.Running)))
                    return BadRequest($"A job for scanning all the publication is already set.");
                job.Publications = null;
            }
            else
            {
                var publications = new List<Publication>();
                foreach (var publication in job.Publications)
                {
                    var p = _context.Publications.Find(publication.ID);
                    if (p != null)
                        publications.Add(p);
                }
                if (publications.Count == 0)
                    return BadRequest("No valid publication(s) ID.");

                job.Publications = publications;

                var currentActiveJobs = _context.LiteratureCrawlingJobs.Where(
                    x => x.Status == State.Queued ||
                         x.Status == State.Running);

                foreach (var activeJob in currentActiveJobs)
                    if (activeJob.Publications != null &&
                        AreListsEqual(activeJob.Publications, job.Publications))
                        return BadRequest(
                            $"The active job {activeJob.ID} has the " +
                            $"same list of publications as the request.");
            }

            _context.LiteratureCrawlingJobs.Add(job);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            Queue.Enqueue(job.ID);
            return CreatedAtAction("GetLiteratureCrawlingJob", new { id = job.ID }, job);
        }

        private bool AreListsEqual(List<Publication> firstList, List<Publication> secondList)
        {
            return !firstList.Except(secondList).Any() && !secondList.Except(firstList).Any();
        }

        private bool LiteratureCrawlingJobExists(int id)
        {
            return _context.LiteratureCrawlingJobs.Any(e => e.ID == id);
        }
    }
}
