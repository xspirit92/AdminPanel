using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CubArt.Infrastructure.Extentions;
using CubArt.Domain.Entities;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Role));

            builder.HasBaseEntityInt();

            builder.PropertyWithUnderscore(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.PropertyWithUnderscore(x => x.Description)
                .HasMaxLength(500);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.Name)
                .IsUnique();
        }
    }

}
