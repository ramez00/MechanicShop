using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Commands.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string UserName,
    string Password,
    string ConfirmPassword,
    string Email,
    string FullName,
    string PhoneNumber) : IRequest<Result<RegisteredCustomerDto>>, ISensitiveRequest;
