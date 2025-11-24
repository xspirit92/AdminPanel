using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(StockMovement));

            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();


            builder.PropertyWithUnderscore(x => x.FacilityId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.ProductId).IsRequired();

            builder.PropertyWithUnderscore(x => x.OperationType).IsRequired();

            builder.PropertyWithUnderscore(x => x.Quantity).IsRequired();

            builder.PropertyWithUnderscore(x => x.ReferenceType).IsRequired();

            builder.PropertyWithUnderscore(x => x.ReferenceId).IsRequired();

            // Внешние ключи
            builder.HasOne(p => p.Facility)
                .WithMany()
                .HasForeignKey(p => p.FacilityId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.FacilityId);

            builder.HasIndexWithUnderscore(x => x.ProductId);

            builder.HasIndexWithUnderscore(x => x.OperationType);

            builder.HasIndexWithUnderscore(x => x.ReferenceType);

            builder.HasIndexWithUnderscore(x => x.ReferenceId);

            builder.HasIndexWithUnderscore(x => x.DateCreated);
        }
    }
}
