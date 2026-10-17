using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Identity;
using MechanicShop.infrastructure.Data;
using MechanicShop.Tests.Common.Constants;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Customers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CustomerAccountLinkTests(WebAppFactory webAppFactory)
{
    private static Customer NewSelfRegistered(string userId)
    {
        var result = Customer.CreateSelfRegistered(
            Guid.NewGuid(),
            userId,
            TestConstants.Customers.Name,
            $"customer.{Guid.NewGuid():N}@example.com",
            TestConstants.Customers.Phone);

        return result.Value;
    }

    private static Customer NewStaffCreated()
        => Customer.Create(
            Guid.NewGuid(),
            TestConstants.Customers.Name,
            $"customer.{Guid.NewGuid():N}@example.com",
            TestConstants.Customers.Phone,
            []).Value;

    [Fact]
    public async Task Two_customers_with_same_UserId_violate_unique_index()
    {
        var userId = Guid.NewGuid().ToString();

        using (var scope = webAppFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Customers.Add(NewSelfRegistered(userId));
            await context.SaveChangesAsync();
        }

        using (var scope = webAppFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Customers.Add(NewSelfRegistered(userId));

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task Many_customers_with_null_UserId_are_allowed()
    {
        using var scope = webAppFactory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Customers.AddRange(NewStaffCreated(), NewStaffCreated());

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task UserId_is_persisted_and_queryable()
    {
        var userId = Guid.NewGuid().ToString();
        var customer = NewSelfRegistered(userId);

        using (var scope = webAppFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
        }

        using (var scope = webAppFactory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var loaded = await context.Customers.AsNoTracking().SingleAsync(c => c.UserId == userId);

            Assert.Equal(customer.Id, loaded.Id);
        }
    }

    [Fact]
    public async Task Customer_role_is_seeded()
    {
        using var scope = webAppFactory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        Assert.True(await roleManager.RoleExistsAsync(RoleNames.Customer));
    }
}
