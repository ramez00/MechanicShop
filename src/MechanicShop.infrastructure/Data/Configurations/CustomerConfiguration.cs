using MechanicShop.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id).IsClustered(false);

        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

        builder.Property(c => c.Email)
               .HasMaxLength(150);

        builder.Navigation(c => c.Cars)
       .UsePropertyAccessMode(PropertyAccessMode.Field);    }
}