using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class AuthorPubEntityTypeConfiguration : IEntityTypeConfiguration<AuthorPublication>
    {
        public void Configure(EntityTypeBuilder<AuthorPublication> builder)
        {
            builder.ToTable("AuthorsPublications");
            builder.HasKey(e => new { e.PublicationID, e.AuthorID });

            builder
                .HasOne(authorPub => authorPub.Author)
                .WithMany(author => author.AuthorPublications)
                .HasForeignKey(authorPub => authorPub.AuthorID);

            builder
                .HasOne(authorPub => authorPub.Publication)
                .WithMany(pub => pub.AuthorPublications)
                .HasForeignKey(authorPub => authorPub.PublicationID);

            //builder.Property(e => e.AuthorID);
            //builder.Property(e => e.PublicationID);
        }
    }
}
