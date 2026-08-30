using MechanicShop.Domain.Customers.cars;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.infrastructure.Data.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.HasKey(v => v.Id).IsClustered(false);

        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Make)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(v => v.Model)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasOne(v => v.Customer)
               .WithMany(c => c.Cars)
               .HasForeignKey(v => v.CustomerId);

        builder.Property(v => v.Year).IsRequired();

        builder.Property(v => v.LicensePlate).IsRequired();
    }
}