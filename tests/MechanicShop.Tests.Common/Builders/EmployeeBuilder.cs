using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Constants;

namespace MechanicShop.Tests.Common.Builders;

public sealed class EmployeeBuilder
{
    private Guid _id = TestConstants.Employees.Id;
    private string _firstName = TestConstants.Employees.FirstName;
    private string _lastName = TestConstants.Employees.LastName;
    private Role _role = Role.Mechanic;

    public EmployeeBuilder WithId(Guid id) { _id = id; return this; }
    public EmployeeBuilder WithFirstName(string firstName) { _firstName = firstName; return this; }
    public EmployeeBuilder WithLastName(string lastName) { _lastName = lastName; return this; }
    public EmployeeBuilder WithRole(Role role) { _role = role; return this; }

    public Employee Build()
    {
        var result = Employee.Create(_id, _firstName, _lastName, _role);
        if (result.IsError)
            throw new InvalidOperationException($"EmployeeBuilder produced an invalid Employee: {result.TopError.Code}");
        return result.Value;
    }
}
