using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Purchase));

            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();


            builder.PropertyWithUnderscore(x => x.SupplierId);
            
            builder.PropertyWithUnderscore(x => x.ProductId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.FacilityId).IsRequired();

            builder.PropertyWithUnderscore(x => x.Quantity).IsRequired();

            builder.PropertyWithUnderscore(x => x.Amount).IsRequired();

            builder.PropertyWithUnderscore(x => x.PurchaseStatus);

            // Внешние ключи
            builder.HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Facility)
                .WithMany()
                .HasForeignKey(p => p.FacilityId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.SupplierId);

            builder.HasIndexWithUnderscore(x => x.ProductId);

            builder.HasIndexWithUnderscore(x => x.FacilityId);

            builder.HasIndexWithUnderscore(x => x.DateCreated);

        }
    }
}
