using FluentValidation;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomer;

public class GetCustomerByIdValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithErrorCode("CustomerId_Is_Required")
            .WithMessage("CustomerId is required.");
    }
}