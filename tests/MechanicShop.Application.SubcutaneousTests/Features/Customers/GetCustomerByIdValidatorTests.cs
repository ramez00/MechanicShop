using MechanicShop.Application.Features.Customers.Queries.GetCustomer;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

namespace MechanicShop.Application.UnitTests.Features.Customers;

public class GetCustomerByIdValidatorTests
{
    private readonly GetCustomerByIdValidator _validator = new();

    [Fact]
    public void Validate_WithId_IsValid()
    {
        Assert.True(_validator.Validate(new GetCustomerByIdQuery(Guid.NewGuid())).IsValid);
    }

    [Fact]
    public void Validate_WithEmptyId_IsInvalidWithExpectedCode()
    {
        var result = _validator.Validate(new GetCustomerByIdQuery(Guid.Empty));

        Assert.Contains(result.Errors, e => e.ErrorCode == "CustomerId_Is_Required");
    }
}
