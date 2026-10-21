using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace MechanicShop.Application.Features.Appointments.Commands.RejectAppointmentRequest;

public sealed class RejectAppointmentRequestCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider,
    HybridCache cache
) : IRequestHandler<RejectAppointmentRequestCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(RejectAppointmentRequestCommand command, CancellationToken ct)
    {
        var appointmentRequest = await _context.AppointmentRequests
            .FirstOrDefaultAsync(ar => ar.Id == command.Id, ct);

        if (appointmentRequest is null)
            return ApplicationErrors.AppointmentNotFound;

        var result = appointmentRequest.Reject(command.ReviewedBy, _timeProvider.GetUtcNow());

        if (result.IsError)
            return result.Errors!;

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("appointment-request", ct);

        return Result.updated;
    }
}
