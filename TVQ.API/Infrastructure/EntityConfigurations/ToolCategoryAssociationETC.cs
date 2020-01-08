using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class ToolCategoryAssociationETC :
        IEntityTypeConfiguration<ToolCategoryAssociation>
    {
        public void Configure(EntityTypeBuilder<ToolCategoryAssociation> builder)
        {
            builder.ToTable("ToolCategoryAssociations");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(ToolRepoAssociation).GetProperties())
            {
                if (p.Name == nameof(ToolCategoryAssociation.ID) ||
                    p.Name == nameof(ToolCategoryAssociation.Tool) ||
                    p.Name == nameof(ToolCategoryAssociation.Category))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
