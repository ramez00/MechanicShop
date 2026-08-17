using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshTokens;

public class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
{
    public RefreshTokenQueryValidator()
    {
        RuleFor(x => x.ExpiredAccessToken)
            .NotEmpty()
            .WithMessage("Expired Access Token is Required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh Token is Required");

    }
}