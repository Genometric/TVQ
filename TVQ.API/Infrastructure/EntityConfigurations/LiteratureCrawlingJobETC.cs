using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class LiteratureCrawlingJobETC : IEntityTypeConfiguration<LiteratureCrawlingJob>
    {
        public void Configure(EntityTypeBuilder<LiteratureCrawlingJob> builder)
        {
            builder.ToTable("LiteratureCrawlingJobs");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(LiteratureCrawlingJob).GetProperties())
            {
                if (p.Name == nameof(LiteratureCrawlingJob.ID) ||
                    p.Name == nameof(LiteratureCrawlingJob.Publications))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
