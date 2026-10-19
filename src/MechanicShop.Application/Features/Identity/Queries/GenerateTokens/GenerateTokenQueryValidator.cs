using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

// Format rules aren't checked here: seeded staff usernames are emails, customer usernames aren't.
public class GenerateTokenQueryValidator : AbstractValidator<GenerateTokenQuery>
{
    public GenerateTokenQueryValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
