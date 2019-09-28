using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class RepositoryItemEntityTypeConfiguration :
        IEntityTypeConfiguration<Repository>
    {
        public void Configure(EntityTypeBuilder<Repository> builder)
        {
            builder.ToTable("Repositories");

            builder.HasKey(obj => obj.ID);

            builder.Property(obj => obj.ID).IsRequired(true);
            builder.Property(obj => obj.URI).IsRequired(true);

            foreach (var p in typeof(Repository).GetProperties())
            {
                if (p.Name == nameof(Repository.ID) ||
                    p.Name == nameof(Repository.URI) ||
                    p.Name == nameof(Repository.Tools))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
