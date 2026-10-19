using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.BookableServices.Dtos;
using MechanicShop.Application.Features.BookableServices.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.BookableServices.Queries.GetBookableServices;

public class GetBookableServicesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetBookableServicesQuery, Result<List<BookableServiceDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<BookableServiceDto>>> Handle(GetBookableServicesQuery query, CancellationToken ct)
    {
        // Parts are loaded only to compute EstimatedTotal; they are not exposed.
        var repairTasks = await _context.RepairTasks
            .AsNoTracking()
            .Include(r => r.Parts)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        return repairTasks.ToBookableServiceDtos();
    }
}
