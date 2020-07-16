using Genometric.TVQ.WebService.Infrastructure;
using Genometric.TVQ.WebService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Controllers
{
    [Route(Program.APIPrefix + "[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly TVQContext _context;

        public CategoriesController(TVQContext context)
        {
            _context = context;
        }

        // GET: api/v1/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync().ConfigureAwait(false);
        }

        // GET: api/v1/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            await _context.Entry(category)
                          .Collection(x => x.ToolAssociations)
                          .LoadAsync()
                          .ConfigureAwait(false);

            await _context.Entry(category)
                          .Collection(x => x.RepoAssociations)
                          .LoadAsync()
                          .ConfigureAwait(false);

            if (category == null)
                return NotFound();

            return category;
        }
    }
}
