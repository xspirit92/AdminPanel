using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(User));

            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();

            builder.PropertyWithUnderscore(x => x.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder.PropertyWithUnderscore(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.PropertyWithUnderscore(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            builder.PropertyWithUnderscore(x => x.FirstName)
                .HasMaxLength(100);

            builder.PropertyWithUnderscore(x => x.LastName)
                .HasMaxLength(100);

            builder.PropertyWithUnderscore(x => x.IsActive);

            builder.PropertyWithUnderscore(x => x.LastLogin)
                .HasDateTimeConversion()
                .HasColumnType(SqlColumnTypes.TimeStampWithTimeZone);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.Username).IsUnique();
            builder.HasIndexWithUnderscore(x => x.Email).IsUnique();
            builder.HasIndexWithUnderscore(x => x.IsActive);
        }
    }

}
