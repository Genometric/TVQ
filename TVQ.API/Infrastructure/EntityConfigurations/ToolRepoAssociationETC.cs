using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class ToolRepoAssociationETC :
        IEntityTypeConfiguration<ToolRepoAssociation>
    {
        public void Configure(EntityTypeBuilder<ToolRepoAssociation> builder)
        {
            builder.ToTable("ToolRepoAssociations");

            builder.HasKey(obi => obi.ID);

            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(ToolRepoAssociation).GetProperties())
            {
                if (p.Name == nameof(ToolRepoAssociation.ID) ||
                    p.Name == nameof(ToolRepoAssociation.Repository) ||
                    p.Name == nameof(ToolRepoAssociation.Tool) ||
                    p.Name == nameof(ToolRepoAssociation.Downloads))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
