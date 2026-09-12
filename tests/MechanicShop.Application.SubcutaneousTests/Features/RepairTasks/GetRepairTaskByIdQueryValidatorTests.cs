using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasksById;

namespace MechanicShop.Application.UnitTests.Features.RepairTasks;

public class GetRepairTaskByIdQueryValidatorTests
{
    private readonly GetRepairTaskByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WithId_IsValid()
    {
        Assert.True(_validator.Validate(new GetRepairTaskByIdQuery(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_IsInvalid()
    {
        var result = _validator.Validate(new GetRepairTaskByIdQuery(Guid.Empty));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetRepairTaskByIdQuery.TaskId));
    }
}
