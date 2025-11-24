using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class ProductionConfiguration : IEntityTypeConfiguration<Production>
    {
        public void Configure(EntityTypeBuilder<Production> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Production));

            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();


            builder.PropertyWithUnderscore(x => x.ProductId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.FacilityId).IsRequired();

            builder.PropertyWithUnderscore(x => x.Quantity).IsRequired();

            // Внешние ключи
            builder.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.ProductId);

            builder.HasIndexWithUnderscore(x => x.FacilityId);

            builder.HasIndexWithUnderscore(x => x.DateCreated);

        }
    }
}
