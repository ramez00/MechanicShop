using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;

public class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
{
    public GetWorkOrderByIdQueryValidator()
    {
        RuleFor(x => x.WorkOrderId)
                .NotEmpty()
                .WithMessage("Work Order Id is required.");
    }
}