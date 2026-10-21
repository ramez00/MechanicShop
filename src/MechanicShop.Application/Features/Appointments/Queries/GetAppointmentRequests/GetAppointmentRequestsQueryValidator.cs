using FluentValidation;

namespace MechanicShop.Application.Features.Appointments.Queries.GetAppointmentRequests;

public sealed class GetAppointmentRequestsQueryValidator
    : AbstractValidator<GetAppointmentRequestsQuery>
{
    public GetAppointmentRequestsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
