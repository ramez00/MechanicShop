using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.Remove;

public class RemoveRepairTaskCommandValidator : AbstractValidator<RemoveRepairTaskCommand>
{
    public RemoveRepairTaskCommandValidator()
    {
        RuleFor(x => x.RepairTaskId).NotEmpty().WithMessage("Repair Task Id is required.");
    }
}