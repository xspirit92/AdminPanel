using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class SupplyConfiguration : IEntityTypeConfiguration<Supply>
    {
        public void Configure(EntityTypeBuilder<Supply> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Supply));

            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();


            builder.PropertyWithUnderscore(x => x.PurchaseId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.Quantity).IsRequired();

            // Внешние ключи
            builder.HasOne(p => p.Purchase)
                .WithMany()
                .HasForeignKey(p => p.PurchaseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.PurchaseId);

            builder.HasIndexWithUnderscore(x => x.DateCreated);

        }
    }
}
