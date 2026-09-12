using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Application.UnitTests.Mappers;

public class LaborMapperTests
{
    [Fact]
    public void ToDto_ShouldMapIdAndFullName()
    {
        var employee = new EmployeeBuilder()
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();

        var dto = employee.ToDto();

        Assert.Equal(employee.Id, dto.LaborId);
        Assert.Equal("Jane Smith", dto.Name);
    }

    [Fact]
    public void ToDtos_ShouldMapEachEmployee()
    {
        var employees = new[]
        {
            new EmployeeBuilder().WithId(Guid.NewGuid()).WithFirstName("Jane").WithLastName("Smith").Build(),
            new EmployeeBuilder().WithId(Guid.NewGuid()).WithFirstName("John").WithLastName("Doe").WithRole(Role.Manager).Build(),
        };

        var dtos = employees.ToDtos();

        Assert.Equal(2, dtos.Count);
        Assert.Equal("Jane Smith", dtos[0].Name);
        Assert.Equal("John Doe", dtos[1].Name);
    }
}
