using MechanicShop.Application.Features.Customers.Commands;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Create.Commands;

public sealed record CreateCustomerCommand(string Name, 
                                            string PhoneNumber,
                                            string Email,
                                            List<CreateCarCommand> cars) : IRequest<Result<CustomerDto>>;