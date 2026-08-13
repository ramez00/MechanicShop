using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts;

public sealed class Part :AuditableEntity
{
    public string? Name { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    private Part() { } // For EF Core

    private Part(Guid id, string name, decimal price, int quantity) : base(id)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public static Result<Part> Create(Guid id, string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PartErrors.NameRequired;

        if (price <= 0 || price > 10000)
            return PartErrors.CostInvalid;

        if (quantity <= 0 || quantity > 10)
            return PartErrors.QuantityInvalid;

        return new Part(id, name, price, quantity);
    }

    public Result<Updated> Update(string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PartErrors.NameRequired;

        if (price <= 0 || price > 10000)
            return PartErrors.CostInvalid;

        if (quantity < 0)
            return PartErrors.QuantityInvalid;

        Name = name;
        Price = price;
        Quantity = quantity;

        return Result.updated;
    }
}