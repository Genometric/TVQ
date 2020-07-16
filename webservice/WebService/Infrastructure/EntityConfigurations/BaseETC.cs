using Genometric.TVQ.WebService.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.Contracts;

namespace Genometric.TVQ.WebService.Infrastructure.EntityConfigurations
{
    public class BaseETC<T> : IEntityTypeConfiguration<T>
        where T : BaseModel
    {
        private readonly string _tableName;

        public BaseETC(string tableName)
        {
            _tableName = tableName;
        }

        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            Contract.Requires(builder != null);

            builder.ToTable(_tableName);
            builder.HasKey(obj => obj.ID);
            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(T).GetProperties())
            {
                if (p.Name == nameof(Repository.ID) ||
                    p.GetGetMethod().IsVirtual)
                    continue;

                builder.Property(p.Name);
            }
        }
    }
}
