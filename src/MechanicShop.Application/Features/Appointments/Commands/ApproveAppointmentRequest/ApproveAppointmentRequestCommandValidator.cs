using FluentValidation;

namespace MechanicShop.Application.Features.Appointments.Commands.ApproveAppointmentRequest;

public sealed class ApproveAppointmentRequestCommandValidator
    : AbstractValidator<ApproveAppointmentRequestCommand>
{
    public ApproveAppointmentRequestCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Appointment request ID is required.");

        RuleFor(x => x.LaborId)
            .NotEmpty().WithMessage("Labor ID is required.");

        RuleFor(x => x.Spot)
            .IsInEnum().WithMessage("Spot must be a valid value.");
    }
}
