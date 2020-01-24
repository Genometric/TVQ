using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class ServiceETC : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.ToTable("Services");
            builder.HasKey(obi => obi.ID);
            builder.Property(obj => obj.ID).IsRequired(true);
            builder.HasIndex(obj => obj.Name).IsUnique();

            foreach (var p in typeof(Service).GetProperties())
            {
                if (p.Name == nameof(Service.ID))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
