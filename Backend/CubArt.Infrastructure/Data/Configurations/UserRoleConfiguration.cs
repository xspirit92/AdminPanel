using CubArt.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CubArt.Infrastructure.Extentions;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(UserRole));

            builder.HasBaseEntityInt();

            builder.PropertyWithUnderscore(x => x.UserId)
                .IsRequired();

            builder.PropertyWithUnderscore(x => x.RoleId)
                .IsRequired();

            // Внешние ключи
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique();
        }
    }

}
