using Genometric.TVQ.API;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TVQ.API.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class PublicationsController : ControllerBase
    {
        private readonly TVQContext _context;

        public PublicationsController(TVQContext context)
        {
            _context = context;
        }

        // GET: api/v1/Publications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublicationDTO>>> GetPublications()
        {
            // As an exception, return List instead of IEnumerable
            // in this API, because the tools count can be more 
            // than the default maximum size of IEnumerable.
            var publications = from publication in _context.Publications
                        select new PublicationDTO(publication);
            return await publications.ToListAsync().ConfigureAwait(false);
        }

        // GET: api/v1/Publications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Publication>> GetPublication(int id)
        {
            /// Instead of `Include(...).FirstOrDefaultAsync(...)` in the following,
            /// we could have used `FindAsync` which checks context before sending 
            /// a query to the database. However, when using `FindAsync` we cannot ask 
            /// to include Publication info of a citation.
            var publication = await
                _context.Publications
                .Include(x => x.Citations)
                .Include(x => x.AuthorAssociations)
                .ThenInclude(x => x.Author)
                .Include(x => x.KeywordAssociations)
                .ThenInclude(x => x.Keyword)
                .FirstOrDefaultAsync(x => x.ID == id)
                .ConfigureAwait(false);

            if (publication == null)
                return NotFound();

            return publication;
        }

        // PUT: api/v1/Publications/5
        // To protect from over-posting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPublication(int id, Publication publication)
        {
            if (id != publication.ID)
            {
                return BadRequest();
            }

            _context.Entry(publication).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublicationExists(id))
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

        // POST: api/v1/Publications
        // To protect from over-posting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Publication>> PostPublication(Publication publication)
        {
            _context.Publications.Add(publication);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return CreatedAtAction("GetPublication", new { id = publication.ID }, publication);
        }

        // DELETE: api/v1/Publications/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Publication>> DeletePublication(int id)
        {
            var publication = await _context.Publications.FindAsync(id);
            if (publication == null)
            {
                return NotFound();
            }

            _context.Publications.Remove(publication);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return publication;
        }

        private bool PublicationExists(int id)
        {
            return _context.Publications.Any(e => e.ID == id);
        }
    }
}
