namespace MechanicShop.Domain.Identity;

// Identity roles that are not employee roles, so they must not live in the Role enum used by Employee.
public static class RoleNames
{
    public const string Customer = "Customer";
}
