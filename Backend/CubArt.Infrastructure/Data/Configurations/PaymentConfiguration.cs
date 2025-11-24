using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Payment));

            builder.HasBaseEntityGuid();
            builder.HasCreatedDateEntity();


            builder.PropertyWithUnderscore(x => x.PurchaseId).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.Amount).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.PaymentMethod);

            builder.PropertyWithUnderscore(x => x.PaymentStatus);

            builder.PropertyWithUnderscore(x => x.Сomment);

            // Внешние ключи
            builder.HasOne(p => p.Purchase)
                .WithMany()
                .HasForeignKey(p => p.PurchaseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Индексы
            builder.HasIndexWithUnderscore(x => x.PurchaseId);

            builder.HasIndexWithUnderscore(x => x.PaymentMethod);

            builder.HasIndexWithUnderscore(x => x.PaymentStatus);

            builder.HasIndexWithUnderscore(x => x.DateCreated);

        }
    }
}
