using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Builders;

public sealed class RepairTaskBuilder
{
    private Guid _id = TestConstants.RepairTasks.Id;
    private string _name = TestConstants.RepairTasks.Name;
    private decimal _laborCost = TestConstants.RepairTasks.LaborCost;
    private RepairDurationInMinutes _duration = RepairDurationInMinutes.Min30;
    private List<Part> _parts = [new PartBuilder().Build()];

    public RepairTaskBuilder WithId(Guid id) { _id = id; return this; }
    public RepairTaskBuilder WithName(string name) { _name = name; return this; }
    public RepairTaskBuilder WithLaborCost(decimal laborCost) { _laborCost = laborCost; return this; }
    public RepairTaskBuilder WithDuration(RepairDurationInMinutes duration) { _duration = duration; return this; }
    public RepairTaskBuilder WithParts(params Part[] parts) { _parts = [.. parts]; return this; }
    public RepairTaskBuilder WithNoParts() { _parts = []; return this; }

    public RepairTask Build()
    {
        var result = RepairTask.Create(_id, _name, _laborCost, _duration, _parts);
        if (result.IsError)
            throw new InvalidOperationException($"RepairTaskBuilder produced an invalid RepairTask: {result.TopError.Code}");
        return result.Value;
    }
}
