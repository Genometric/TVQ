using Genometric.TVQ.Infrastructure.EntityConfigurations;
using Genometric.TVQ.Model;
using Microsoft.EntityFrameworkCore;

namespace Genometric.TVQ.Infrastructure
{
    public class RepoItemContext : DbContext
    {
        public RepoItemContext(
            DbContextOptions<RepoItemContext> options) : base(options)
        {

        }

        public DbSet<RepoItem> Repos { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new RepoItemEntityTypeConfiguration());
        }
    }
}
