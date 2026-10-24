using MechanicShop.Domain.AppointmentRequests;
using MechanicShop.Domain.AppointmentRequests.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.infrastructure.Data.Configurations;

public class AppointmentRequestConfiguration : IEntityTypeConfiguration<AppointmentRequest>
{
    public void Configure(EntityTypeBuilder<AppointmentRequest> builder)
    {
        builder.HasKey(a => a.Id).IsClustered(false);

        builder.Property(a => a.Status)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(a => a.RequestedStartAtUtc).IsRequired();
        builder.Property(a => a.RequestedEndAtUtc).IsRequired();

        builder.Property(a => a.Notes)
               .HasMaxLength(500);

        builder.Property(a => a.ReviewedBy)
               .HasMaxLength(450);

        builder.Property(a => a.RowVersion)
               .IsRowVersion();

        // FK to Customer — restrict to prevent accidental cascade
        builder.HasOne<Domain.Customers.Customer>()
               .WithMany()
               .HasForeignKey(a => a.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK to Car
        builder.HasOne<Domain.Customers.cars.Car>()
               .WithMany()
               .HasForeignKey(a => a.CarId)
               .OnDelete(DeleteBehavior.Restrict);

        // FK to WorkOrder (nullable — set when approved)
        builder.HasOne<Domain.WorkOrders.WorkOrder>()
               .WithMany()
               .HasForeignKey(a => a.WorkOrderId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        // Many-to-many with RepairTasks
        builder.HasMany(a => a.RepairTasks)
               .WithMany()
               .UsingEntity(j => j.ToTable("AppointmentRequestRepairTasks"));

        // Composite index for availability queries: filter by status + date range
        builder.HasIndex(a => new { a.Status, a.RequestedStartAtUtc });

        builder.HasIndex(a => a.CustomerId);
    }
}
