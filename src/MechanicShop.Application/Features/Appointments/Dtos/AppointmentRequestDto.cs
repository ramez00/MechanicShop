namespace MechanicShop.Application.Features.Appointments.Dtos;

public sealed record AppointmentRequestDto
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string Vehicle { get; init; } = string.Empty;
    public DateTimeOffset RequestedStartAt { get; init; }
    public DateTimeOffset RequestedEndAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public List<string> Services { get; init; } = [];
    public string? Notes { get; init; }
    public string? ReviewedBy { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }
}
