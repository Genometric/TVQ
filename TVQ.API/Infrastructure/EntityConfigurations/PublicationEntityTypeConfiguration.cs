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

            builder.HasKey(obi => obi.Id);

            builder.Property(obj => obj.Id).IsRequired(true);

            foreach (var p in typeof(Publication).GetProperties())
            {
                if (p.Name == nameof(Publication.Id))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
