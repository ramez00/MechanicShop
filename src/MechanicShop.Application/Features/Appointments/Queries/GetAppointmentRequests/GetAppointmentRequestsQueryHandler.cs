using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Appointments.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Appointments.Queries.GetAppointmentRequests;

public sealed class GetAppointmentRequestsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAppointmentRequestsQuery, Result<PaginatedList<AppointmentRequestDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<PaginatedList<AppointmentRequestDto>>> Handle(
        GetAppointmentRequestsQuery query, CancellationToken ct)
    {
        var dbQuery = _context.AppointmentRequests.AsNoTracking()
            .Include(ar => ar.RepairTasks)
            .AsQueryable();

        if (query.Status.HasValue)
            dbQuery = dbQuery.Where(ar => ar.Status == query.Status.Value);

        if (query.From.HasValue)
        {
            var fromOffset = new DateTimeOffset(query.From.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            dbQuery = dbQuery.Where(ar => ar.RequestedStartAtUtc >= fromOffset);
        }

        if (query.To.HasValue)
        {
            var toOffset = new DateTimeOffset(query.To.Value.ToDateTime(TimeOnly.MinValue).AddDays(1), TimeSpan.Zero);
            dbQuery = dbQuery.Where(ar => ar.RequestedStartAtUtc < toOffset);
        }

        var count = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .OrderByDescending(ar => ar.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Join(_context.Vehicles, ar => ar.CarId, v => v.Id, (ar, v) => new { ar, v })
            .Join(_context.Customers, x => x.ar.CustomerId, c => c.Id, (x, c) => new { x.ar, x.v, c })
            .Select(x => new AppointmentRequestDto
            {
                Id = x.ar.Id,
                CustomerName = x.c.Name,
                Vehicle = x.v.Make + " | " + x.v.LicensePlate,
                RequestedStartAt = x.ar.RequestedStartAtUtc,
                RequestedEndAt = x.ar.RequestedEndAtUtc,
                Status = x.ar.Status.ToString(),
                Services = x.ar.RepairTasks.Select(rt => rt.Name).ToList(),
                Notes = x.ar.Notes,
                ReviewedBy = x.ar.ReviewedBy,
                ReviewedAt = x.ar.ReviewedAt
            })
            .ToListAsync(ct);

        return new PaginatedList<AppointmentRequestDto>
        {
            Items = items,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = count,
            TotalPages = (int)Math.Ceiling(count / (double)query.PageSize)
        };
    }
}
