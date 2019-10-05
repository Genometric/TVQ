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

            foreach (var p in typeof(Tool).GetProperties())
            {
                if (p.Name == nameof(Tool.ID) ||
                    p.Name == nameof(Tool.Repository) ||
                    p.Name == nameof(Tool.Publications) ||
                    p.Name == nameof(Tool.Downloads))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
