using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
    {
        public void Configure(EntityTypeBuilder<StockBalance> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(StockBalance));

            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();


            builder.PropertyWithUnderscore(x => x.FacilityId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.ProductId).IsRequired();

            builder.PropertyWithUnderscore(x => x.StartBalance);

            builder.PropertyWithUnderscore(x => x.IncomeBalance);

            builder.PropertyWithUnderscore(x => x.OutcomeBalance);

            builder.PropertyWithUnderscore(x => x.FinishBalance);

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

            builder.HasIndexWithUnderscore(x => x.DateCreated);


            builder.HasIndex(x => new { x.FacilityId, x.ProductId, x.DateCreated })
                .HasDatabaseName("idx_stock_balance_facility_id_product_id_date_created")
                .IsUnique();
        }
    }
}
