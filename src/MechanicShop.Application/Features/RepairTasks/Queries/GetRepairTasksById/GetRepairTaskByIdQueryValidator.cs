using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasksById;

public class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
{
    public GetRepairTaskByIdQueryValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("Task Id is Required.");
    }
}