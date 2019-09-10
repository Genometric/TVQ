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

            builder.HasKey(obi => obi.Id);

            builder.Property(obj => obj.Id).IsRequired(true);
            builder.HasOne(obj => obj.Repo)
                .WithMany()
                .HasForeignKey(obj => obj.RepositoryID);

            foreach (var p in typeof(Tool).GetProperties())
            {
                if (p.Name == nameof(Tool.Id) ||
                    p.Name == nameof(Tool.Repo))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
