using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.Remove;

public class RemoveCommandValidator : AbstractValidator<RemoveCustomerCommand>
{
    public RemoveCommandValidator()
    {
        RuleFor(c => c.customerId)
            .NotEmpty()
            .WithMessage("Customer Id Is required");
    }
}