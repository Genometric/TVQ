using Genometric.TVQ.API.Model.Associations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class PublicationKeywordAssociationETC : IEntityTypeConfiguration<PublicationKeywordAssociation>
    {
        public void Configure(EntityTypeBuilder<PublicationKeywordAssociation> builder)
        {
            builder.ToTable("PublicationKeywordAssociations");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(PublicationKeywordAssociation).GetProperties())
            {
                if (p.Name == nameof(PublicationKeywordAssociation.ID) ||
                    p.Name == nameof(PublicationKeywordAssociation.Publication) ||
                    p.Name == nameof(PublicationKeywordAssociation.Keyword))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
