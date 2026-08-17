using FluentValidation;
using MechanicShop.Application.Features.Customers.Cars;

namespace MechanicShop.Application.Features.Customers.Create.Commands;
public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be in E.164 format.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.cars)
            .NotNull().WithMessage("Cars list cannot be null.")
            .Must(cars => cars.Count > 0).WithMessage("At least one car must be provided.");    

        RuleForEach(x => x.cars)
            .SetValidator(new CreateCarValidator());
    }
}