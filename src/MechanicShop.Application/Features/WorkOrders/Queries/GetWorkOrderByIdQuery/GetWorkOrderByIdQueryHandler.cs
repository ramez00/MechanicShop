using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;

public class GetWorkOrderByIdQueryHandler(
    IAppDbContext context
) : IRequestHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
    private readonly IAppDbContext _context = context;
    public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await  _context.WorkOrders
                                        .Include(w => w.RepairTasks)
                                        .ThenInclude(r => r.Parts)
                                        .Include(w => w.Labor)
                                        .Include(w => w.Car)
                                        .ThenInclude(c => c!.Customer)
                                        .Include(w => w.Invoice)
                                        .FirstOrDefaultAsync(a => a.Id == request.WorkOrderId,cancellationToken);

        if(workOrder is null)
            return ApplicationErrors.WorkOrderNotFound;                                

        return workOrder.ToDto();
    }
}