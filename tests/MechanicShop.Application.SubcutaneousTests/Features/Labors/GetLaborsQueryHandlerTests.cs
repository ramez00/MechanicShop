using MechanicShop.Application.Features.Labors.Queries;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Application.UnitTests.Features.Labors;

public class GetLaborsQueryHandlerTests : HandlerTestBase
{
    private GetLaborsQueryHandler CreateSut() => new(Context);

    [Fact]
    public async Task Handle_ReturnsOnlyMechanics()
    {
        await SeedAsync(
            new EmployeeBuilder().WithId(Guid.NewGuid()).WithRole(Role.Mechanic).Build(),
            new EmployeeBuilder().WithId(Guid.NewGuid()).WithRole(Role.Manager).Build(),
            new EmployeeBuilder().WithId(Guid.NewGuid()).WithRole(Role.Receptionist).Build());
        var sut = CreateSut();

        var result = await sut.Handle(new GetLaborsQuery(), CancellationToken.None);

        var labors = result.ShouldBeSuccess();
        Assert.Single(labors);
    }

    [Fact]
    public async Task Handle_WhenNoMechanics_ReturnsEmpty()
    {
        await SeedAsync(new EmployeeBuilder().WithId(Guid.NewGuid()).WithRole(Role.Manager).Build());
        var sut = CreateSut();

        var result = await sut.Handle(new GetLaborsQuery(), CancellationToken.None);

        Assert.Empty(result.ShouldBeSuccess());
    }
}
