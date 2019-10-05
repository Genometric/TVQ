using Genometric.TVQ.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Genometric.TVQ.API.Infrastructure.EntityConfigurations
{
    public class ToolDownloadRecordEntityTypeConfiguration :
        IEntityTypeConfiguration<ToolDownloadRecord>
    {
        public void Configure(EntityTypeBuilder<ToolDownloadRecord> builder)
        {
            builder.ToTable("ToolDownloadRecords");

            builder.HasKey(obi => obi.ID);

            builder.Property(obj => obj.ID).IsRequired(true);

            foreach (var p in typeof(ToolDownloadRecord).GetProperties())
            {
                if (p.Name == nameof(ToolDownloadRecord.ID) ||
                    p.Name == nameof(ToolDownloadRecord.Tool))
                    continue;
                builder.Property(p.Name);
            }
        }
    }
}
