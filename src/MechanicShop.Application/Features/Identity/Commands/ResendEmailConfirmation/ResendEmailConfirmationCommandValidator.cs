using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.ResendEmailConfirmation;

public class ResendEmailConfirmationCommandValidator : AbstractValidator<ResendEmailConfirmationCommand>
{
    public ResendEmailConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
