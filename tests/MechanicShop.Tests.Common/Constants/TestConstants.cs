namespace MechanicShop.Tests.Common.Constants;

/// <summary>
/// Fixed values shared across tests so intent is obvious and results are deterministic.
/// </summary>
public static class TestConstants
{
    /// <summary>Canonical "now" used by the fixed <see cref="Fakes.FixedTimeProvider"/>.</summary>
    public static readonly DateTimeOffset UtcNow = new(2026, 01, 15, 09, 00, 00, TimeSpan.Zero);

    public static class Customers
    {
        public static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public const string Name = "John Doe";
        public const string Email = "john.doe@example.com";
        public const string Phone = "+12025550123";
    }

    public static class Cars
    {
        public static readonly Guid Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public const string Make = "Toyota";
        public const string Model = "Corolla";
        public const int Year = 2020;
        public const string LicensePlate = "ABC-1234";
    }

    public static class Employees
    {
        public static readonly Guid Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public const string FirstName = "Jane";
        public const string LastName = "Smith";
    }

    public static class RepairTasks
    {
        public static readonly Guid Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        public const string Name = "Oil Change";
        public const decimal LaborCost = 120m;
    }

    public static class Parts
    {
        public static readonly Guid Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
        public const string Name = "Oil Filter";
        public const decimal Price = 25m;
        public const int Quantity = 1;
    }

    public static class WorkOrders
    {
        public static readonly Guid Id = Guid.Parse("66666666-6666-6666-6666-666666666666");
    }

    public static class Invoices
    {
        public static readonly Guid Id = Guid.Parse("77777777-7777-7777-7777-777777777777");
    }
}
