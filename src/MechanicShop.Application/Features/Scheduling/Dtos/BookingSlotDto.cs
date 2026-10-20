namespace MechanicShop.Application.Features.Scheduling.Dtos;

public record BookingSlotDto(DateTimeOffset StartAt, DateTimeOffset EndAt);
