using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class StatisticsETC : IEntityTypeConfiguration<Statistics>
    {
        public void Configure(EntityTypeBuilder<Statistics> builder)
        {
            builder.ToTable("Statistics");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(Statistics).GetProperties())
            {
                if (p.Name == nameof(Statistics.ID) ||
                    p.Name == nameof(Statistics.Repository))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
