using Genometric.TVQ.WebService.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.Contracts;

namespace Genometric.TVQ.WebService.Infrastructure.EntityConfigurations
{
    public class ServiceETC : BaseETC<Service>
    {
        public ServiceETC() : base("Services")
        { }

        public override void Configure(EntityTypeBuilder<Service> builder)
        {
            Contract.Requires(builder != null);

            base.Configure(builder);
            builder.HasIndex(obj => obj.Name).IsUnique();
        }
    }
}
