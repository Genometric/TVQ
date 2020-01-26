using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TVQ.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CitationsController : ControllerBase
    {
        private readonly TVQContext _context;
        private readonly IBaseBackgroundTaskQueue<LiteratureCrawlingJob> _queue;
        private readonly ILogger<CitationsController> _logger;

        public CitationsController(
            TVQContext context,
            IBaseBackgroundTaskQueue<LiteratureCrawlingJob> queue,
            ILogger<CitationsController> logger)
        {
            _context = context;
            _queue = queue;
            _logger = logger;
        }

        // GET: api/v1/Citations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Citation>>> GetCitations()
        {
            return Ok(await _context.Citations.ToListAsync().ConfigureAwait(false));
        }

        // GET: api/v1/Citations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCitation(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            /// Instead of `Include(...).FirstOrDefaultAsync(...)` in the following,
            /// we could have used `FindAsync` which checks context before sending 
            /// a query to the database. However, when using `FindAsync` we cannot ask 
            /// to include Publication info of a citation.
            var citation = await
                _context.Citations
                .Include(x => x.Publication)
                .FirstOrDefaultAsync(x => x.ID == id).ConfigureAwait(false);

            if (citation == null)
            {
                return NotFound();
            }

            return Ok(citation);
        }

        // PUT: api/v1/Citations/5
        // To protect from over-posting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCitation(int id, Citation citation)
        {
            if (id != citation.ID)
            {
                return BadRequest();
            }

            _context.Entry(citation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CitationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/v1/Citations
        // To protect from over-posting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Citation>> PostCitation(Citation citation)
        {
            _context.Citations.Add(citation);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return CreatedAtAction("GetCitation", new { id = citation.ID }, citation);
        }

        // DELETE: api/v1/Citations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Citation>> DeleteCitation(int id)
        {
            var citation = await _context.Citations.FindAsync(id);
            if (citation == null)
            {
                return NotFound();
            }

            _context.Citations.Remove(citation);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return citation;
        }

        private bool CitationExists(int id)
        {
            return _context.Citations.Any(e => e.ID == id);
        }
    }
}
