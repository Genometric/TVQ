using Genometric.TVQ.API.Infrastructure.EntityConfigurations;
using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;

namespace Genometric.TVQ.API.Infrastructure
{
    public class TVQContext : DbContext
    {
        public TVQContext(
            DbContextOptions<TVQContext> options) : base(options)
        {

        }

        public DbSet<Tool> Tools { set; get; }
        public DbSet<Repository> Repositories { set; get; }
        public DbSet<Publication> Publications { set; get; }
        public DbSet<Citation> Citations { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ToolEntityTypeConfiguration());
            builder.ApplyConfiguration(new PublicationEntityTypeConfiguration());
            builder.ApplyConfiguration(new RepositoryItemEntityTypeConfiguration());
            builder.ApplyConfiguration(new CitationEntityTypeConfiguration());
        }
    }
}
