using FluentValidation;
using FluentValidation.Validators;

namespace MechanicShop.Application.Features.Customers.Commands.Update;

public class UpdateCarCommandValidator : AbstractValidator<UpdateCarCommand>
{
    public UpdateCarCommandValidator()
    {
         RuleFor(x => x.Make)
            .NotEmpty().MaximumLength(50);

        RuleFor(x => x.Model)
            .NotEmpty().MaximumLength(50);

        RuleFor(x => x.LicensePlate)
            .NotEmpty().MaximumLength(10);
    }
}