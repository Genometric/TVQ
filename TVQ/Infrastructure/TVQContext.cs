using Genometric.TVQ.Infrastructure.EntityConfigurations;
using Genometric.TVQ.Model;
using Microsoft.EntityFrameworkCore;

namespace Genometric.TVQ.Infrastructure
{
    public class TVQContext : DbContext
    {
        public TVQContext(
            DbContextOptions<TVQContext> options) : base(options)
        {

        }

        public DbSet<Tool> Tools { set; get; }
        public DbSet<Repository> Repositories { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ToolEntityTypeConfiguration());
            builder.ApplyConfiguration(new RepositoryItemEntityTypeConfiguration());
        }
    }
}
