using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class PublicationEntityTypeConfiguration :
        IEntityTypeConfiguration<Publication>
    {
        public void Configure(EntityTypeBuilder<Publication> builder)
        {
            builder.ToTable("Publications");

            builder.HasKey(obi => obi.ID);

            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(Publication).GetProperties())
            {
                if (p.Name == nameof(Publication.ID) ||
                    p.Name == nameof(Publication.Tool) ||
                    p.Name == nameof(Publication.Citations) ||
                    p.Name == nameof(Publication.Authors) ||
                    p.Name == nameof(Publication.Keywords))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
