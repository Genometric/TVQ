using Genometric.TVQ.API.Model.Associations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class AuthorPublicationAssociationsETC : IEntityTypeConfiguration<AuthorPublicationAssociation>
    {
        public void Configure(EntityTypeBuilder<AuthorPublicationAssociation> builder)
        {
            builder.ToTable("AuthorsPublicationAssociations");

            builder.HasKey(obi => obi.ID);

            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(AuthorPublicationAssociation).GetProperties())
            {
                if (p.Name == nameof(AuthorPublicationAssociation.ID) ||
                    p.Name == nameof(AuthorPublicationAssociation.Publication) ||
                    p.Name == nameof(AuthorPublicationAssociation.Author))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
