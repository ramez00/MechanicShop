using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Domain.UnitTests.Employees;

public class EmployeeTests
{
    // ---------- Create ----------

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccess()
    {
        var result = Employee.Create(TestConstants.Employees.Id, "Jane", "Smith", Role.Mechanic);

        var employee = result.ShouldBeSuccess();
        Assert.Equal("Jane", employee.FirstName);
        Assert.Equal("Smith", employee.LastName);
        Assert.Equal(Role.Mechanic, employee.Role);
    }

    [Fact]
    public void Create_WithEmptyId_ReturnsIdRequired()
    {
        var result = Employee.Create(Guid.Empty, "Jane", "Smith", Role.Mechanic);

        result.ShouldBeError(EmployeeErrors.IdRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankFirstName_ReturnsFirstNameRequired(string firstName)
    {
        var result = Employee.Create(TestConstants.Employees.Id, firstName, "Smith", Role.Mechanic);

        result.ShouldBeError(EmployeeErrors.FirstNameRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithBlankLastName_ReturnsLastNameRequired(string lastName)
    {
        var result = Employee.Create(TestConstants.Employees.Id, "Jane", lastName, Role.Mechanic);

        result.ShouldBeError(EmployeeErrors.LastNameRequired);
    }

    [Fact]
    public void Create_WithUndefinedRole_ReturnsRoleInvalid()
    {
        var result = Employee.Create(TestConstants.Employees.Id, "Jane", "Smith", (Role)99);

        result.ShouldBeError(EmployeeErrors.RoleInvalid);
    }

    // ---------- FullName ----------

    [Fact]
    public void FullName_ConcatenatesFirstAndLast()
    {
        var employee = new EmployeeBuilder().WithFirstName("Jane").WithLastName("Smith").Build();

        Assert.Equal("Jane Smith", employee.FullName);
    }
}
