using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class ProductSpecificationItemConfiguration : IEntityTypeConfiguration<ProductSpecificationItem>
    {
        public void Configure(EntityTypeBuilder<ProductSpecificationItem> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(ProductSpecificationItem));

            builder.HasBaseEntityInt();


            builder.PropertyWithUnderscore(x => x.ProductSpecificationId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.ProductId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.Quantity).IsRequired();

            // Внешние ключи
            builder.HasOne(p => p.ProductSpecification)
                .WithMany(p => p.Items)
                .HasForeignKey(p => p.ProductSpecificationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.ProductSpecificationId);
        }
    }
}
