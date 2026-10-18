namespace MechanicShop.Contracts.Requests.Identity;

// Validation rules live in RegisterCustomerCommandValidator (single source of truth).
public class RegisterCustomerRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
