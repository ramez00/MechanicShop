using FluentValidation;
using FluentValidation.Validators;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.userId).NotEmpty().WithMessage("User Id is Required.");
    }
}