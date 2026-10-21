using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.AppointmentRequests.Enums;
using MechanicShop.Domain.workOrders.Enums;
using MechanicShop.infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MechanicShop.infrastructure.BackgroundJobs;

public class OverdueBookingCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<OverdueBookingCleanupService> logger,
    IOptions<AppSettings> options,
    TimeProvider dateTime
) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OverdueBookingCleanupService> _logger = logger;
    private readonly TimeProvider _dateTime = dateTime;
    private readonly AppSettings _appSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_appSettings.OverdueBookingCleanupFrequencyMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
             _logger.LogInformation("Checking overdue work orders at {Now}", _dateTime.GetUtcNow());

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                var cutoff = _dateTime.GetUtcNow().AddMinutes(-_appSettings.BookingCancellationThresholdMinutes);

                var overdue = await db.WorkOrders
                                      .Where(w => w.State == WorkOrderState.Scheduled && w.StartAtUtc <= cutoff)
                                      .ToListAsync(stoppingToken);

                if(overdue.Count > 0)
                {
                    foreach (var wo in overdue)
                    {
                        var result = wo.Cancel();

                        if(result.IsError)
                            _logger.LogWarning("Failed to cancel WorkOrder {Id}: {Error}", wo.Id, result.Errors);

                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Cancelled {Count} overdue work orders: {Ids}", overdue.Count, overdue.Select(w => w.Id));   
                    }
                }
                else
                    _logger.LogInformation("No overdue work orders found.");

                // Expire overdue pending appointment requests
                var utcNow = _dateTime.GetUtcNow();

                var overdueAppointments = await db.AppointmentRequests
                    .Where(ar => ar.Status == AppointmentRequestStatus.Pending
                              && ar.RequestedEndAtUtc < utcNow)
                    .ToListAsync(stoppingToken);

                if (overdueAppointments.Count > 0)
                {
                    foreach (var ar in overdueAppointments)
                    {
                        var expireResult = ar.Expire();

                        if (expireResult.IsError)
                            _logger.LogWarning("Failed to expire AppointmentRequest {Id}: {Error}", ar.Id, expireResult.Errors);
                    }

                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Expired {Count} overdue appointment requests: {Ids}",
                        overdueAppointments.Count, overdueAppointments.Select(a => a.Id));
                }
                else
                    _logger.LogInformation("No overdue appointment requests found.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up overdue work orders.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_appSettings.OverdueBookingCleanupFrequencyMinutes),stoppingToken);
        }
    }
}