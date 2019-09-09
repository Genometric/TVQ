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
    public class RepositoriesController : ControllerBase
    {
        private readonly TVQContext _context;

        public RepositoriesController(
            TVQContext context)
        {
            _context = context;
        }

        // GET: api/repositories
        [HttpGet]
        public IEnumerable<Repository> GetDatas()
        {
            return _context.Repositories;
        }

        // GET: api/repositories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var DataItem = await _context.Tools.FindAsync(id);

            if (DataItem == null)
            {
                return NotFound();
            }

            return Ok(DataItem);
        }

        // PUT: api/repositories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDataItem([FromRoute] int id, [FromBody] Tool DataItem)
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

        // POST: api/repositories
        [HttpPost]
        public async Task<IActionResult> PostDataItem([FromBody] Tool DataItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Tools.Add(DataItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequestItems", new { }, DataItem);
        }

        // DELETE: api/repositories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var DataItem = await _context.Tools.FindAsync(id);
            if (DataItem == null)
            {
                return NotFound();
            }

            _context.Tools.Remove(DataItem);
            await _context.SaveChangesAsync();

            return Ok(DataItem);
        }

        private bool DataItemExists(int id)
        {
            return _context.Tools.Any(e => e.ID == id);
        }
    }
}