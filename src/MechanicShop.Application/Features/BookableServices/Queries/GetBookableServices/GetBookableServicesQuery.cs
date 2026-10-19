using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.BookableServices.Dtos;
using MechanicShop.Application.Features.RepairTasks;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.BookableServices.Queries.GetBookableServices;

// The same list for every customer, so one cache entry serves them all.
public sealed record GetBookableServicesQuery : ICachedQuery<Result<List<BookableServiceDto>>>
{
    public string CacheKey => "bookable-services";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

    public string[] Tags => [RepairTaskCacheTags.RepairTask];
}
