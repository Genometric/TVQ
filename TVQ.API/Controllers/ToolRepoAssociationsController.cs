using Genometric.TVQ.API;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model.Associations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TVQ.API.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class ToolRepoAssociationsController : ControllerBase
    {
        private readonly TVQContext _context;

        public ToolRepoAssociationsController(TVQContext context)
        {
            _context = context;
        }

        // GET: api/ToolRepoAssociations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolRepoAssociation>>> GetToolRepoAssociation()
        {
            return await _context.ToolRepoAssociation.ToListAsync();
        }

        // GET: api/ToolRepoAssociations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ToolRepoAssociation>> GetToolRepoAssociation(int id)
        {
            var toolRepoAssociation = await _context.ToolRepoAssociation.FindAsync(id);

            if (toolRepoAssociation == null)
            {
                return NotFound();
            }

            return toolRepoAssociation;
        }

        // PUT: api/ToolRepoAssociations/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutToolRepoAssociation(int id, ToolRepoAssociation toolRepoAssociation)
        {
            if (id != toolRepoAssociation.ID)
            {
                return BadRequest();
            }

            _context.Entry(toolRepoAssociation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToolRepoAssociationExists(id))
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

        // POST: api/ToolRepoAssociations
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<ToolRepoAssociation>> PostToolRepoAssociation(ToolRepoAssociation toolRepoAssociation)
        {
            _context.ToolRepoAssociation.Add(toolRepoAssociation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetToolRepoAssociation", new { id = toolRepoAssociation.ID }, toolRepoAssociation);
        }

        // DELETE: api/ToolRepoAssociations/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ToolRepoAssociation>> DeleteToolRepoAssociation(int id)
        {
            var toolRepoAssociation = await _context.ToolRepoAssociation.FindAsync(id);
            if (toolRepoAssociation == null)
            {
                return NotFound();
            }

            _context.ToolRepoAssociation.Remove(toolRepoAssociation);
            await _context.SaveChangesAsync();

            return toolRepoAssociation;
        }

        private bool ToolRepoAssociationExists(int id)
        {
            return _context.ToolRepoAssociation.Any(e => e.ID == id);
        }
    }
}
