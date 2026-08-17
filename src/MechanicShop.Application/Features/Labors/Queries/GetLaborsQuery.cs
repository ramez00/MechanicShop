using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Labors.Queries;

public sealed record GetLaborsQuery : ICachedQuery<Result<List<LaborDto>>>
{
       public string CacheKey => $"labors";
    public string[] Tags => ["labors"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

}