using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.customers.Mappers;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Create.Commands;
public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<CreateCustomerHandler> _logger;
    private readonly HybridCache _cache;

    public CreateCustomerHandler(IAppDbContext dbContext, ILogger<CreateCustomerHandler> logger,HybridCache cache)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var userEmail = request.Email.Trim().ToLower();

        var isEmailExists = await _dbContext.Customers.AnyAsync(c => c.Email!.ToLower() == userEmail, cancellationToken);

        if (isEmailExists)
        {
            _logger.LogWarning($"Create Customer with Email already defined {request.Email}");
            return CustomerErrors.CustomerAlreadyExists;
        }

        List<Car> cars = [];

        foreach(var car in request.cars)
        {
            var customerCar = Car.Create(Guid.NewGuid(),car.Make,car.Model,car.Year,car.LicensePlate);

            if (customerCar.IsError)
            {
                _logger.LogWarning("Error occurred when Car Created");
                return customerCar.Errors!;
            }

            cars.Add(customerCar.Value);    
        }

        var customer = Customer.Create(Guid.NewGuid(),request.Name.Trim(),request.Email.Trim(),request.PhoneNumber.Trim(),cars);

        if (customer.IsError)
        {
            _logger.LogWarning("Error occurred when Customer Created ");
            return customer.Errors!;

        }

        _dbContext.Customers.Add(customer.Value);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByTagAsync("customer",cancellationToken);

        return customer.Value.ToDto();    
    }
}