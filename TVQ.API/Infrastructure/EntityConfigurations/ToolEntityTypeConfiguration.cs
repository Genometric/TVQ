using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class ToolEntityTypeConfiguration :
        IEntityTypeConfiguration<Tool>
    {
        public void Configure(EntityTypeBuilder<Tool> builder)
        {
            builder.ToTable("Tools");

            builder.HasKey(obi => obi.ID);

            builder.Property(obj => obj.ID).IsRequired(true);

            builder.HasIndex(obj => obj.Name).IsUnique();

            foreach (var p in typeof(Tool).GetProperties())
            {
                if (p.Name == nameof(Tool.ID) ||
                    p.Name == nameof(Tool.RepoAssociations) ||
                    p.Name == nameof(Tool.Publications))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
