using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class KeywordEntityTypeConfiguration : IEntityTypeConfiguration<Keyword>
    {
        public void Configure(EntityTypeBuilder<Keyword> builder)
        {
            builder.ToTable("Keywords");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(Keyword).GetProperties())
            {
                if (p.Name == nameof(Keyword.ID) ||
                    p.Name == nameof(Keyword.Publication))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
