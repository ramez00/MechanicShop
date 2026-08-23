using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
{
    public RelocateWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty();

        RuleFor(x => x.NewStartAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("New start time must be in the future.");

        RuleFor(x => x.NewSpot)
            .IsInEnum();
    }
}