using Genometric.TVQ.API.Model.Associations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class ToolPublicationAssociationETC :
        IEntityTypeConfiguration<ToolPublicationAssociation>
    {
        public void Configure(EntityTypeBuilder<ToolPublicationAssociation> builder)
        {
            builder.ToTable("ToolPublicationAssociations");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(ToolPublicationAssociation).GetProperties())
            {
                if (p.Name == nameof(ToolPublicationAssociation.ID) ||
                    p.Name == nameof(ToolPublicationAssociation.Tool) ||
                    p.Name == nameof(ToolPublicationAssociation.Publication))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
