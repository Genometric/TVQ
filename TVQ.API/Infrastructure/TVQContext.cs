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
        public DbSet<ToolDownloadRecord> ToolDownloadRecords { set; get; }
        public DbSet<Repository> Repositories { set; get; }
        public DbSet<Publication> Publications { set; get; }
        public DbSet<Citation> Citations { set; get; }
        public DbSet<Author> Authors { set; get; }
        public DbSet<Keyword> Keywords { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ToolEntityTypeConfiguration());
            builder.ApplyConfiguration(new PublicationEntityTypeConfiguration());
            builder.ApplyConfiguration(new RepositoryItemEntityTypeConfiguration());
            builder.ApplyConfiguration(new CitationEntityTypeConfiguration());
            builder.ApplyConfiguration(new ToolDownloadRecordEntityTypeConfiguration());
            builder.ApplyConfiguration(new AuthorEntityTypeConfiguration());
            builder.ApplyConfiguration(new KeywordEntityTypeConfiguration());
            builder.ApplyConfiguration(new AuthorPubEntityTypeConfiguration());
        }
    }
}
