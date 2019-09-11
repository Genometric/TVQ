using Genometric.TVQ.API.Crawlers;
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
                return BadRequest(ModelState);

            var DataItem = await _context.Repositories.FindAsync(id);
            if (DataItem == null)
                return NotFound();

            return Ok(DataItem);
        }

        // PUT: api/repositories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDataItem([FromRoute] int id, [FromBody] Repository dataItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dataItem.Id)
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

        // POST: api/repositories
        [HttpPost]
        public async Task<IActionResult> PostDataItem([FromBody] Repository dataItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Repositories.Add(dataItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRequestItems", new { }, dataItem);
        }

        // DELETE: api/repositories/5
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

        // GET: api/repositories/scan/1
        [HttpGet("{id}/scan")]
        public async Task<IActionResult> ScanToolsInRepo([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dataItem = await _context.Repositories.FindAsync(id);
            if (dataItem == null)
                return NotFound();

            /// TODO: Can use `ConfigureAwait(false)` in the following to 
            /// request getting a separate thread for the following task.
            /// However, since it is not a process-bound task, it may not 
            /// be necessary. However, it shall be further investigated.
            var tools = await new Crawler().CrawlAsync(dataItem);

            try
            {
                await _context.Tools.AddRangeAsync(tools);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DataItemExists(id))
                    return NotFound();
                else
                    throw;
            }

            return Ok(dataItem);
        }

        private bool DataItemExists(int id)
        {
            return _context.Repositories.Any(e => e.Id == id);
        }
    }
}