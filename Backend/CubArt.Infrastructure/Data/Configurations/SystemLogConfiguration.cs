using CubArt.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CubArt.Infrastructure.Extentions;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
    {
        public void Configure(EntityTypeBuilder<SystemLog> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(SystemLog));
            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();

            builder.PropertyWithUnderscore(x => x.Level).HasMaxLength(20);
            builder.PropertyWithUnderscore(x => x.Message).HasColumnType(SqlColumnTypes.Text);
            builder.PropertyWithUnderscore(x => x.UserId).HasMaxLength(100);
            builder.PropertyWithUnderscore(x => x.IpAddress).HasMaxLength(50);
            builder.PropertyWithUnderscore(x => x.UserAgent).HasMaxLength(100);
            builder.PropertyWithUnderscore(x => x.RequestPath).HasMaxLength(1000);
            builder.PropertyWithUnderscore(x => x.RequestMethod).HasMaxLength(1000);
            builder.PropertyWithUnderscore(x => x.ExceptionType).HasMaxLength(1000);
            builder.PropertyWithUnderscore(x => x.Source).HasMaxLength(100);
            builder.PropertyWithUnderscore(x => x.Action).HasMaxLength(50);
            builder.PropertyWithUnderscore(x => x.EntityType).HasMaxLength(100);
            builder.PropertyWithUnderscore(x => x.EntityId).HasMaxLength(50);
            builder.PropertyWithUnderscore(x => x.AdditionalData).HasColumnType(SqlColumnTypes.JsonB);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.Level);
            builder.HasIndexWithUnderscore(x => x.DateCreated);
            builder.HasIndexWithUnderscore(x => x.Source);
            builder.HasIndexWithUnderscore(x => x.UserId);
        }
    }

}
