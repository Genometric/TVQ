using Genometric.TVQ.API.Model;
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

            //builder.HasOne(x => x.Publication).WithMany().OnDelete(DeleteBehavior.NoAction).HasForeignKey(x => x.PublicationID);
            //builder.HasOne(x => x.Keyword).WithMany().OnDelete(DeleteBehavior.NoAction).HasForeignKey(x => x.KeywordID);

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
