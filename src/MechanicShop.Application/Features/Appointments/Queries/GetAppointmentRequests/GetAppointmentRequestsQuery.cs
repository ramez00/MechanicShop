using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Appointments.Dtos;
using MechanicShop.Domain.AppointmentRequests.Enums;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Appointments.Queries.GetAppointmentRequests;

public sealed record GetAppointmentRequestsQuery(
    AppointmentRequestStatus? Status,
    DateOnly? From,
    DateOnly? To,
    int Page,
    int PageSize
) : ICachedQuery<Result<PaginatedList<AppointmentRequestDto>>>
{
    public string CacheKey =>
        $"appointment-requests:p={Page}:ps={PageSize}" +
        $":status={Status?.ToString() ?? "-"}" +
        $":from={From?.ToString("yyyyMMdd") ?? "-"}" +
        $":to={To?.ToString("yyyyMMdd") ?? "-"}";

    public string[] Tags => ["appointment-request"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
