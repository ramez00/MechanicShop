using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.Remove;

public class RemoveCommandHandler(IAppDbContext context , ILogger<RemoveCommandHandler> logger,HybridCache cache) 
: IRequestHandler<RemoveCommand, Result<Deleted>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<RemoveCommandHandler> _logger =logger;
    private readonly HybridCache _cache =cache;

    public async Task<Result<Deleted>> Handle(RemoveCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FindAsync(request.customerId);

        if(customer is null)
        {
            _logger.LogWarning("Customer with id {CustomerId} not found for deletion.", request.customerId);
            return ApplicationErrors.CustomerNotFound;
        }

        var hasWorkOrder = await _context.WorkOrders
                                         .Include(w => w.Car)
                                         .Where(wo => wo.Car != null)
                                         .AnyAsync(wo => wo.Car!.CustomerId == request.customerId);
        
        if (hasWorkOrder)
        {
            _logger.LogWarning("Customer {CustomerId} cannot be deleted because they have associated work orders (past, scheduled, or in-progress).", request.customerId);
            return CustomerErrors.CannotDeleteCustomerWithActiveOrders;
        }

        _context.Customers.Remove(customer);

        await _context.SaveChangesAsync(cancellationToken);      

        await _cache.RemoveByTagAsync("customer", cancellationToken);

         _logger.LogInformation("Customer {CustomerId} deleted successfully.", request.customerId);

        return Result.deleted;
    }
}