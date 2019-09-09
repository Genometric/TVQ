using Genometric.TVQ.Infrastructure;
using Genometric.TVQ.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolShedsController : ControllerBase
    {
        private readonly RepoItemContext _context;

        public ToolShedsController(
            RepoItemContext context)
        {
            _context = context;
        }

        // GET: api/toolsheds
        [HttpGet]
        public IEnumerable<RepoItem> GetDatas()
        {
            var repos = _context.Repos;
            return repos;
        }

        // GET: api/toolsheds/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var DataItem = await _context.Repos.FindAsync(id);

            if (DataItem == null)
            {
                return NotFound();
            }

            return Ok(DataItem);
        }

        // PUT: api/toolsheds/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDataItem([FromRoute] int id, [FromBody] RepoItem DataItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != DataItem.ID)
            {
                return BadRequest();
            }

            _context.Entry(DataItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DataItemExists(id))
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

        // POST: api/toolsheds
        [HttpPost]
        public async Task<IActionResult> PostDataItem([FromBody] RepoItem DataItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Repos.Add(DataItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequestItems", new { }, DataItem);
        }

        // DELETE: api/toolsheds/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var DataItem = await _context.Repos.FindAsync(id);
            if (DataItem == null)
            {
                return NotFound();
            }

            _context.Repos.Remove(DataItem);
            await _context.SaveChangesAsync();

            return Ok(DataItem);
        }

        private bool DataItemExists(int id)
        {
            return _context.Repos.Any(e => e.ID == id);
        }
    }
}