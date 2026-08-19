using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers;

public static class CustomerErrors
{
    public static Error NameRequired => Error.Validation("Customer name is required.");
    public static Error EmailRequired => Error.Validation("Customer email is required.");
    public static Error PhoneNumberRequired => Error.Validation("Customer phone number is required.");
    public static Error EmailInvalid => Error.Validation("Customer email is invalid.");
    public static Error CustomerAlreadyExists => Error.Conflict($"Customer with email already exists.");
    public static Error CustomerNotFound(Guid customerId) => Error.NotFound($"Customer with ID '{customerId}' was not found.");
    public static readonly Error CannotDeleteCustomerWithActiveOrders = Error.Conflict("Cannot delete customer with active orders.");
}