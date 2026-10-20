using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetAvailability;

public sealed record GetAvailabilityQuery(
    DateOnly Date,
    List<Guid> RepairTaskIds,
    TimeZoneInfo TimeZone
) : ICachedQuery<Result<AvailabilityResponse>>
{
    public string CacheKey =>
        $"availability:{Date:yyyy-MM-dd}:tz={TimeZone.Id}:{string.Join(",", RepairTaskIds.OrderBy(x => x))}";

    public string[] Tags => ["work-order"];

    public TimeSpan Expiration => TimeSpan.FromSeconds(30);
}
