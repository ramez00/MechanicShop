using FluentValidation;

namespace MechanicShop.Application.Features.MyVehicles.Commands.RemoveMyVehicle;

public class RemoveMyVehicleCommandValidator : AbstractValidator<RemoveMyVehicleCommand>
{
    public RemoveMyVehicleCommandValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Vehicle id is required.");
    }
}
