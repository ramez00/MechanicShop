using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.workOrders.Enums;
using MediatR;

namespace MechanicShop.Application.Features.Dashboard.Queries;

public class GetWorkOrderStatsQueryHandler(IAppDbContext context) : IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
{
    private readonly IAppDbContext _context = context;
    public async Task<Result<TodayWorkOrderStatsDto>> Handle(GetWorkOrderStatsQuery request, CancellationToken cancellationToken)
    {
         var start = request.date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = request.date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var query = _context.WorkOrders
                            .Include(w => w.Car)
                            .Include(w => w.RepairTasks).ThenInclude(r => r.Parts)
                            .Include(r => r.Invoice)
                            .Where(w => w.StartAtUtc >= start && w.StartAtUtc < end);

        var total = await query.CountAsync(cancellationToken);

        if (total == 0)
        {
            return new TodayWorkOrderStatsDto
            {
                Date = request.date,
                Total = 0,
                Scheduled = 0,
                InProgress = 0,
                Completed = 0,
                Cancelled = 0,
                TotalRevenue = 0,
                TotalPartsCost = 0,
                TotalLaborCost = 0,
                UniqueVehicles = 0,
                UniqueCustomers = 0
            };
        }   

        var stats = await query.ToListAsync(cancellationToken);

        var totalRevenue = stats.Sum(x => x.Invoice?.Total ?? 0);
        var totalPartCost = stats.Where(x => x.Invoice != null).Sum(x => x.TotalPartsCost ?? 0);
        var totalLaborCost = stats.Where(x => x.Invoice != null).Sum(x => x.TotalLaborCost ?? 0);
        var uniqueCars = stats.Select(x => x.CarId).Distinct().Count();
        var uniqueCustomers = stats.Select(x => x.Car!.CustomerId).Distinct().Count();

        var netProfit = totalRevenue - totalPartCost - totalLaborCost;

        return new TodayWorkOrderStatsDto
        {
            Date = request.date,
            Total = total,
            Scheduled = stats.Count(x => x.State == WorkOrderState.Scheduled),
            InProgress = stats.Count(x => x.State == WorkOrderState.InProgress),
            Completed = stats.Count(x => x.State == WorkOrderState.Completed),
            Cancelled = stats.Count(x => x.State == WorkOrderState.Cancelled),
            TotalRevenue = totalRevenue,
            TotalPartsCost = totalPartCost,
            TotalLaborCost = totalLaborCost,
            UniqueVehicles = uniqueCars,
            UniqueCustomers = uniqueCustomers,
            NetProfit = netProfit,
            ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
            CompletionRate = total > 0 ? ((decimal)stats.Count(x => x.State == WorkOrderState.Completed) / total) * 100 : 0,
            AverageRevenuePerOrder = total > 0 ? totalRevenue / total : 0,
            OrdersPerVehicle = uniqueCars > 0 ? (decimal)total / uniqueCars : 0,
            PartsCostRatio = totalRevenue > 0 ? (totalPartCost / totalRevenue) * 100 : 0,
            LaborCostRatio = totalRevenue > 0 ? (totalLaborCost / totalRevenue) * 100 : 0,
            CancellationRate = total > 0 ? ((decimal)stats.Count(x => x.State == WorkOrderState.Cancelled) / total) * 100 : 0
        };
    }
}