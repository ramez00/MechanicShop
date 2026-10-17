namespace MechanicShop.Application.Common.Interfaces;

public interface IUser
{
    string? Id {get;}

    // Set only for users in the Customer role (from the customer_id token claim).
    Guid? CustomerId {get;}
}
