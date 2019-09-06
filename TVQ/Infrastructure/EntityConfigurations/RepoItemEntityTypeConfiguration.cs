using Genometric.TVQ.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.Infrastructure.EntityConfigurations
{
    public class RepoItemEntityTypeConfiguration :
        IEntityTypeConfiguration<RepoItem>
    {
        public void Configure(EntityTypeBuilder<RepoItem> builder)
        {
            builder.ToTable("Repos");

            builder.HasKey(ci => ci.ID);
            builder.Property(ci => ci.ID).IsRequired(true);
        }
    }
}
