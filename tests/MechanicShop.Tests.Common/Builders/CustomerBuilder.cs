using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.cars;
using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Builders;

public sealed class CustomerBuilder
{
    private Guid _id = TestConstants.Customers.Id;
    private string _name = TestConstants.Customers.Name;
    private string _email = TestConstants.Customers.Email;
    private string _phone = TestConstants.Customers.Phone;
    private List<Car> _cars = [new CarBuilder().Build()];

    public CustomerBuilder WithId(Guid id) { _id = id; return this; }
    public CustomerBuilder WithName(string name) { _name = name; return this; }
    public CustomerBuilder WithEmail(string email) { _email = email; return this; }
    public CustomerBuilder WithPhone(string phone) { _phone = phone; return this; }
    public CustomerBuilder WithCars(params Car[] cars) { _cars = [.. cars]; return this; }
    public CustomerBuilder WithNoCars() { _cars = []; return this; }

    public Customer Build()
    {
        var result = Customer.Create(_id, _name, _email, _phone, _cars);
        if (result.IsError)
            throw new InvalidOperationException($"CustomerBuilder produced an invalid Customer: {result.TopError.Code}");
        return result.Value;
    }
}
