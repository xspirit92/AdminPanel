using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Supplier));

            builder.HasBaseEntityInt();


            builder.PropertyWithUnderscore(x => x.Name).IsRequired();
        }
    }
}
