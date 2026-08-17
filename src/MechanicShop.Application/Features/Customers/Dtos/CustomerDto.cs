namespace MechanicShop.Application.Features.Customers.Dtos;

public sealed record CustomerDto(Guid Id, 
                                   string Name, 
                                   string PhoneNumber, 
                                   string Email, 
                                   List<CarDto> Cars);