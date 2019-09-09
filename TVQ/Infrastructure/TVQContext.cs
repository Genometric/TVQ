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

        public DbSet<RepoItem> RepoItems { set; get; }
        public DbSet<ToolShedItem> ToolShedItems { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new RepoItemEntityTypeConfiguration());
            builder.ApplyConfiguration(new ToolShedItemEntityTypeConfiguration());
        }
    }
}
