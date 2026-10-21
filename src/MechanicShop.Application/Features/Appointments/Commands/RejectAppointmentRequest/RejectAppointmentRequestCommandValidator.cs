using FluentValidation;

namespace MechanicShop.Application.Features.Appointments.Commands.RejectAppointmentRequest;

public sealed class RejectAppointmentRequestCommandValidator
    : AbstractValidator<RejectAppointmentRequestCommand>
{
    public RejectAppointmentRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Appointment request ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
