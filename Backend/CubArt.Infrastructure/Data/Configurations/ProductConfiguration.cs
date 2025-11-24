using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Product));

            builder.HasBaseEntityInt();


            builder.PropertyWithUnderscore(x => x.Name).IsRequired();

            builder.PropertyWithUnderscore(x => x.ProductType).IsRequired();

            builder.PropertyWithUnderscore(x => x.UnitOfMeasure);

            // Внешние ключи
            builder.HasMany(p => p.ProductSpecifications)
                .WithOne(p => p.Product)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.Name).IsUnique();
            builder.HasIndexWithUnderscore(x => x.ProductType);
        }
    }
}
