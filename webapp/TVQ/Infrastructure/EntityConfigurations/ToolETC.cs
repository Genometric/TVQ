using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.Contracts;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class ToolETC : BaseETC<Tool>
    {
        public ToolETC() : base("Tools")
        { }

        public override void Configure(EntityTypeBuilder<Tool> builder)
        {
            Contract.Requires(builder != null);

            base.Configure(builder);
            builder.HasIndex(obj => obj.Name).IsUnique();
        }
    }
}
