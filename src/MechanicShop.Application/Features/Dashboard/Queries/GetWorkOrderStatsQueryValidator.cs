using FluentValidation;
using FluentValidation.Validators;

namespace MechanicShop.Application.Features.Dashboard.Queries;

public class GetWorkOrderStatsQueryValidator : AbstractValidator<GetWorkOrderStatsQuery>
{
    public GetWorkOrderStatsQueryValidator()
    {
        RuleFor(q => q.date).NotEmpty().WithMessage("Date is Required .");
    }
}