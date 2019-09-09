using Genometric.TVQ.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.Infrastructure.EntityConfigurations
{
    public class ToolEntityTypeConfiguration :
        IEntityTypeConfiguration<Tool>
    {
        public void Configure(EntityTypeBuilder<Tool> builder)
        {
            builder.ToTable("Tools");

            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);
        }
    }
}
