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
        public DbSet<Category> Categories { set; get; }
        public DbSet<Service> Services { set; get; }
        public DbSet<ToolRepoAssociation> ToolRepoAssociation { get; set; }
        public DbSet<RepoCrawlingJob> RepoCrawlingJobs { set; get; }
        public DbSet<LiteratureCrawlingJob> LiteratureCrawlingJobs { set; get; }
        public DbSet<AnalysisJob> AnalysisJobs { set; get; }
        public DbSet<Statistics> Statistics { set; get; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ToolEntityTypeConfiguration());
            builder.ApplyConfiguration(new PublicationEntityTypeConfiguration());
            builder.ApplyConfiguration(new RepositoryItemEntityTypeConfiguration());
            builder.ApplyConfiguration(new ToolRepoAssociationETC());
            builder.ApplyConfiguration(new CitationEntityTypeConfiguration());
            builder.ApplyConfiguration(new ToolDownloadRecordEntityTypeConfiguration());
            builder.ApplyConfiguration(new AuthorEntityTypeConfiguration());
            builder.ApplyConfiguration(new KeywordEntityTypeConfiguration());
            builder.ApplyConfiguration(new AuthorPubEntityTypeConfiguration());
            builder.ApplyConfiguration(new StatisticsEntityTypeConfiguration());
            builder.ApplyConfiguration(new CategoryETC());
            builder.ApplyConfiguration(new ServiceETC());
            builder.ApplyConfiguration(new RepoCrawlingJobETC());
            builder.ApplyConfiguration(new LiteratureCrawlingJobETC());
            builder.ApplyConfiguration(new AnalysisJobETC());
        }
    }
}
