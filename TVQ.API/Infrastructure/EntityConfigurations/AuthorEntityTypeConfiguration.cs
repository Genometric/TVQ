using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class AuthorEntityTypeConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.ToTable("Authors");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            builder.Property(e => e.FirstName);
            builder.Property(e => e.LastName);

            foreach (var p in typeof(Author).GetProperties())
            {
                if (p.Name == nameof(Author.ID) ||
                    p.Name == nameof(Author.AuthorPublications))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
