using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Controllers
{
    [Route("api/v1/[controller]")]
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

        // POST: api/Services
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Service>> PostService(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return CreatedAtAction("GetService", new { id = service.ID }, service);
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ID == id);
        }
    }
}
