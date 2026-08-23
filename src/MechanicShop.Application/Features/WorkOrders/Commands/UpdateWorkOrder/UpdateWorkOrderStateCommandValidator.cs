using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrder;

public class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderStateCommand>
{
    public UpdateWorkOrderStateCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty()
            .WithMessage("Work Order ID is Required");

        RuleFor(x => x.WorkOrderState)
            .IsInEnum()
            .WithMessage("Status must be a valid WorkOrderStatus value.");
;    
    }
}