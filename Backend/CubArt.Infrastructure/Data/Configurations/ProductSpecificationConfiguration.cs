using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
    {
        public void Configure(EntityTypeBuilder<ProductSpecification> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(ProductSpecification));

            builder.HasBaseEntityInt();
            builder.HasCreatedDateEntity();


            builder.PropertyWithUnderscore(x => x.ProductId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.Version);
            
            builder.PropertyWithUnderscore(x => x.IsActive);

            builder.PropertyWithUnderscore(x => x.DateCreated);

            // Внешние ключи
            builder.HasOne(p => p.Product)
                .WithMany(p => p.ProductSpecifications)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(p => p.Items)
                .WithOne(p => p.ProductSpecification)
                .HasForeignKey(p => p.ProductSpecificationId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.ProductId);
        }
    }
}
