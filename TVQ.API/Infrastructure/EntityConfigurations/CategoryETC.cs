using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class CategoryETC : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);
            builder.HasIndex(obj => obj.Name).IsUnique();

            foreach (var p in typeof(Category).GetProperties())
            {
                if (p.Name == nameof(Category.ID) ||
                    p.Name == nameof(Category.ToolAssociations))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
