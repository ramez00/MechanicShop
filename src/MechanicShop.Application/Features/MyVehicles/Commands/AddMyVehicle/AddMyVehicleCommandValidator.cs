using FluentValidation;

namespace MechanicShop.Application.Features.MyVehicles.Commands.AddMyVehicle;

// The model-year range is a domain rule (Car.Create), so it isn't repeated here.
public class AddMyVehicleCommandValidator : AbstractValidator<AddMyVehicleCommand>
{
    public AddMyVehicleCommandValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required.")
            .MaximumLength(50).WithMessage("Make cannot exceed 50 characters.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(50).WithMessage("Model cannot exceed 50 characters.");

        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("License plate is required.")
            // Measured after normalization, so spaces the customer typed do not count.
            .Must(p => string.IsNullOrEmpty(p) || Car.NormalizeLicensePlate(p).Length <= 10).WithMessage("License plate cannot exceed 10 characters.");
    }
}
