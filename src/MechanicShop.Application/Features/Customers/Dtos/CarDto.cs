namespace MechanicShop.Application.Features.Customers.Dtos;

public sealed record CarDto(Guid Id, 
                               string Make, 
                               string Model, 
                               int Year, 
                               string LicensePlate);