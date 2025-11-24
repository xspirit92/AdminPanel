using CubArt.Domain.Entities;
using CubArt.Infrastructure.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CubArt.Infrastructure.Data.Configurations
{
    public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
    {
        public void Configure(EntityTypeBuilder<Facility> builder)
        {
            builder.HasTableNameUnderscoreStyle(nameof(Facility));

            builder.HasBaseEntityInt();


            builder.PropertyWithUnderscore(x => x.Name).IsRequired();
            
            builder.PropertyWithUnderscore(x => x.Address);
        }
    }
}
