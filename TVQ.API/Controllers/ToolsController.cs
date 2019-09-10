using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly TVQContext _context;

        public ToolsController(
            TVQContext context)
        {
            _context = context;
        }

        // GET: api/tools
        [HttpGet]
        public IEnumerable<Tool> GetDatas()
        {
            return _context.Tools;
        }

        // GET: api/tools/5
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

        // PUT: api/tools/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDataItem([FromRoute] int id, [FromBody] Tool DataItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != DataItem.Id)
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

        // POST: api/tools
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

        // DELETE: api/tools/5
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
            return _context.Tools.Any(e => e.Id == id);
        }
    }
}