using Genometric.TVQ.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.Infrastructure.EntityConfigurations
{
    public class ToolShedItemEntityTypeConfiguration :
        IEntityTypeConfiguration<ToolShedItem>
    {
        public void Configure(EntityTypeBuilder<ToolShedItem> builder)
        {
            builder.ToTable("ToolSheds");

            builder.HasKey(ci => ci.ID);
            builder.Property(ci => ci.ID).IsRequired(true);
        }
    }
}
