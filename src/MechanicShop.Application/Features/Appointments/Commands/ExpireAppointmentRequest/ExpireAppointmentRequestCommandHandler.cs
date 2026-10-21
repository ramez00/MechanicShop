using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.Appointments.Commands.ExpireAppointmentRequest;

public sealed class ExpireAppointmentRequestCommandHandler(
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<ExpireAppointmentRequestCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(ExpireAppointmentRequestCommand command, CancellationToken ct)
    {
        var appointmentRequest = await _context.AppointmentRequests
            .FirstOrDefaultAsync(ar => ar.Id == command.Id, ct);

        if (appointmentRequest is null)
            return ApplicationErrors.AppointmentNotFound;

        var result = appointmentRequest.Expire();

        if (result.IsError)
            return result.Errors!;

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("appointment-request", ct);

        return Result.updated;
    }
}
