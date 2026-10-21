using FluentValidation;

namespace MechanicShop.Application.Features.Appointments.Commands.SubmitAppointmentRequest;

public sealed class SubmitAppointmentRequestCommandValidator
    : AbstractValidator<SubmitAppointmentRequestCommand>
{
    public SubmitAppointmentRequestCommandValidator()
    {
        RuleFor(x => x.CarId)
            .NotEmpty().WithMessage("Car ID is required.");

        RuleFor(x => x.RepairTaskIds)
            .NotEmpty().WithMessage("At least one repair task must be selected.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}
