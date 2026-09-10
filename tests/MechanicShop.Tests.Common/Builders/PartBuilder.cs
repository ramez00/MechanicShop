using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Builders;

public sealed class PartBuilder
{
    private Guid _id = TestConstants.Parts.Id;
    private string _name = TestConstants.Parts.Name;
    private decimal _price = TestConstants.Parts.Price;
    private int _quantity = TestConstants.Parts.Quantity;

    public PartBuilder WithId(Guid id) { _id = id; return this; }
    public PartBuilder WithName(string name) { _name = name; return this; }
    public PartBuilder WithPrice(decimal price) { _price = price; return this; }
    public PartBuilder WithQuantity(int quantity) { _quantity = quantity; return this; }

    public Part Build()
    {
        var result = Part.Create(_id, _name, _price, _quantity);
        if (result.IsError)
            throw new InvalidOperationException($"PartBuilder produced an invalid Part: {result.TopError.Code}");
        return result.Value;
    }
}
