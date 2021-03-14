using Genometric.TVQ.WebService.Infrastructure;
using Genometric.TVQ.WebService.Model;
using Genometric.TVQ.WebService.Model.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly TVQContext _context;

        public ToolsController(
            TVQContext context)
        {
            _context = context;
        }

        // GET: api/v1/tools
        [HttpGet]
        public List<ToolDTO> GetTools()
        {
            // As an exception, return List instead of IEnumerable
            // in this API, because the tools count can be more
            // than the default maximum size of IEnumerable.
            var tools = from tool in _context.Tools
                        select new ToolDTO(tool);
            return tools.ToList();
        }

        // GET: api/v1/tools/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTool([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var DataItem = await _context.Tools
                .Include(x => x.PublicationAssociations)
                .Include(x => x.RepoAssociations)
                .Include(x => x.CategoryAssociations)
                    .ThenInclude(x => x.Category)
                .FirstAsync(x => x.ID == id)
                .ConfigureAwait(false);

            if (DataItem == null)
            {
                return NotFound();
            }

            return Ok(DataItem);
        }

        // PUT: api/v1/tools/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTool([FromRoute] int id, [FromBody] Tool DataItem)
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
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToolExists(id))
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

        // POST: api/v1/tools
        [HttpPost]
        public async Task<IActionResult> PostTool([FromBody] Tool DataItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Tools.Add(DataItem);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return CreatedAtAction("GetRequestItems", new { }, DataItem);
        }

        // DELETE: api/v1/tools/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool([FromRoute] int id)
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
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(DataItem);
        }

        private bool ToolExists(int id)
        {
            return _context.Tools.Any(e => e.ID == id);
        }
    }
}