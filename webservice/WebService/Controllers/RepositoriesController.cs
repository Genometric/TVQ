using Genometric.TVQ.WebService.Infrastructure;
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
    public class RepositoriesController : ControllerBase
    {
        private readonly TVQContext _context;
        private readonly ILogger<RepositoriesController> _logger;

        public RepositoriesController(
            TVQContext context,
            ILogger<RepositoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/v1/repositories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Repository>>> GetRepos()
        {
            return await
                _context.Repositories
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

        private bool RepoExists(int id)
        {
            return _context.Repositories.Any(e => e.ID == id);
        }
    }
}