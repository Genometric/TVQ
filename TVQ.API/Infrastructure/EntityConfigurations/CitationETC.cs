using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class CitationETC : IEntityTypeConfiguration<Citation>
    {
        public void Configure(EntityTypeBuilder<Citation> builder)
        {
            builder.ToTable("Citations");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(Citation).GetProperties())
            {
                if (p.Name == nameof(Citation.ID) ||
                    p.Name == nameof(Citation.Publication))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
