using Genometric.TVQ.WebService.Infrastructure;
using Genometric.TVQ.WebService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly TVQContext _context;

        public ServicesController(TVQContext context)
        {
            _context = context;
        }

        // GET: api/v1/Services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            return await _context.Services.ToListAsync().ConfigureAwait(false);
        }

        // GET: api/v1/Services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();
            return service;
        }

        // PUT: api/v1/Services/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutService([FromRoute] int id, [FromBody] Service service)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != service.ID)
                return BadRequest();

            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
                    return NotFound();
                else
                    throw;
            }

            return CreatedAtAction("GetService", new { id = service.ID }, service);
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ID == id);
        }
    }
}
