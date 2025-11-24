using CubArt.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CubArt.Infrastructure.Extentions;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(RolePermission));

            builder.HasBaseEntityInt();

            builder.PropertyWithUnderscore(x => x.RoleId)
                .IsRequired();

            builder.PropertyWithUnderscore(x => x.Permission)
                .IsRequired();

            // Внешние ключи
            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndex(x => new { x.RoleId, x.Permission })
                .IsUnique();
        }
    }

}
