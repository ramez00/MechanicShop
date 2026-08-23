using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.AssignLabor;

public class AssignLaborCommandValidator : AbstractValidator<AssignLaborCommand>
{
    public AssignLaborCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
           .NotEmpty()
           .WithMessage("Work Order ID Required");

        RuleFor(x => x.LaborId)
           .NotEmpty()
           .WithMessage("Labor Id is Required");  
    }
}