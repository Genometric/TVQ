using Genometric.TVQ.Infrastructure.EntityConfigurations;
using Genometric.TVQ.Model;
using Microsoft.EntityFrameworkCore;

namespace Genometric.TVQ.Infrastructure
{
    public class ToolShedItemContext : DbContext
    {
        public ToolShedItemContext(
            DbContextOptions<ToolShedItemContext> options) : base(options)
        {

        }

        public DbSet<ToolShedItem> ToolSheds { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ToolShedItemEntityTypeConfiguration());
        }
    }
}
