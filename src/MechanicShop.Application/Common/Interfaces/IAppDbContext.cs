using MechanicShop.Domain.AppointmentRequests;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MechanicShop.Application.Common.Interfaces;

public interface IAppDbContext
{

    public DbSet<Customer> Customers { get; }
    public DbSet<Part> Parts { get; }
    public DbSet<RepairTask> RepairTasks { get; }
    public DbSet<Car> Vehicles { get; }
    public DbSet<WorkOrder> WorkOrders { get; }
    public DbSet<Employee> Employees { get; }
    public DbSet<Invoice> Invoices { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    public DbSet<AppointmentRequest> AppointmentRequests { get; }

    // Exposed for explicit transactions that must also cover Identity writes (UserManager shares this context).
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}