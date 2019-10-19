using Genometric.TVQ.API.Crawlers;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks;
using Genometric.TVQ.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genometric.TVQ.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RepositoriesController : ControllerBase
    {
        private readonly TVQContext _context;
        private readonly IBackgroundCrawlingQueue _queue;
        private readonly ILogger<RepositoriesController> _logger;

        public RepositoriesController(
            TVQContext context,
            IBackgroundCrawlingQueue queue,
            ILogger<RepositoriesController> logger)
        {
            _context = context;
            _queue = queue;
            _logger = logger;
        }

        // GET: api/v1/repositories
        [HttpGet]
        public IEnumerable<Repository> GetDatas()
        {
            return _context.Repositories;
        }

        // GET: api/v1/repositories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDataItem([FromRoute] int id)
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
        public async Task<IActionResult> PutDataItem([FromRoute] int id, [FromBody] Repository dataItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dataItem.ID)
                return BadRequest();

            _context.Entry(dataItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DataItemExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/v1/repositories
        [HttpPost]
        public async Task<IActionResult> PostDataItem([FromBody] Repository dataItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Repositories.Add(dataItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequestItems", new { }, dataItem);
        }

        // DELETE: api/v1/repositories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dataItem = await _context.Repositories.FindAsync(id);
            if (dataItem == null)
                return NotFound();

            _context.Repositories.Remove(dataItem);
            await _context.SaveChangesAsync();

            return Ok(dataItem);
        }

        // GET: api/v1/repositories/scan/1
        [HttpGet("{id}/scan")]
        public async Task<IActionResult> ScanToolsInRepo([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var repository = await _context.Repositories.FindAsync(id);
            if (repository == null)
                return NotFound();
            if (!DataItemExists(id))
                return NotFound();

            _queue.QueueBackgroundWorkItem(repository);

            return Ok(repository);
        }

        private bool DataItemExists(int id)
        {
            return _context.Repositories.Any(e => e.ID == id);
        }
    }
}