namespace MechanicShop.Application.Features.Scheduling.Dtos;

public record AvailabilityResponse(IReadOnlyList<BookingSlotDto> Slots, string? Message);
