using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.Update;

public class UpdateCustomerCommandHandler(ILogger<UpdateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache) 
    : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{

    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
                                     .Include(c => c.Cars)
                                     .FirstOrDefaultAsync(c => c.Id == request.CustomerId);

        
        if (customer is null)
        {
            _logger.LogWarning("Customer {CustomerId} not found for update.", request.CustomerId);

            return ApplicationErrors.CustomerNotFound;
        }
                             
        List<Car> customerCars = [];

        foreach(var c in request.Cars)
        {
            var carId = c.CarId ?? Guid.NewGuid();

            var car = Car.Create(carId,c.Make,c.Model,c.Year,c.LicensePlate);

            if (car.IsError)
                return car.Errors!;

            customerCars.Add(car.Value);    
        }

        var updatedCustomer = customer.Update(request.Name,request.Email,request.PhoneNumber);

        if(updatedCustomer.IsError)
            return updatedCustomer.Errors!;

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("customer",cancellationToken);

        return Result.updated;    
    }
}