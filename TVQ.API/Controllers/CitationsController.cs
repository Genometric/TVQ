using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TVQ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitationsController : ControllerBase
    {
        private readonly TVQContext _context;

        public CitationsController(TVQContext context)
        {
            _context = context;
        }

        // GET: api/Citations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Citation>>> GetCitations()
        {
            return await _context.Citations.ToListAsync().ConfigureAwait(false);
        }

        // GET: api/Citations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Citation>> GetCitation(int id)
        {
            var citation = await _context.Citations.FindAsync(id);

            if (citation == null)
            {
                return NotFound();
            }

            return citation;
        }

        // PUT: api/Citations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
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

        // POST: api/Citations
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Citation>> PostCitation(Citation citation)
        {
            _context.Citations.Add(citation);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return CreatedAtAction("GetCitation", new { id = citation.ID }, citation);
        }

        // DELETE: api/Citations/5
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
