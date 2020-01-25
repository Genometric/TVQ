using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class AnalysisJobETC : IEntityTypeConfiguration<AnalysisJob>
    {
        public void Configure(EntityTypeBuilder<AnalysisJob> builder)
        {
            builder.ToTable("AnalysisJobs");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(AnalysisJob).GetProperties())
            {
                if (p.Name == nameof(AnalysisJob.ID) ||
                    p.Name == nameof(AnalysisJob.Repository))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
