using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class RepoCrawlingJobETC : IEntityTypeConfiguration<RepoCrawlingJob>
    {
        public void Configure(EntityTypeBuilder<RepoCrawlingJob> builder)
        {
            builder.ToTable("RepoCrawlingJobs");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(RepoCrawlingJob).GetProperties())
            {
                if (p.Name == nameof(RepoCrawlingJob.ID) ||
                    p.Name == nameof(RepoCrawlingJob.Repository))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
