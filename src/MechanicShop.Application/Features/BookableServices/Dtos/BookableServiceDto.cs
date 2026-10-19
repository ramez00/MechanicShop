namespace MechanicShop.Application.Features.BookableServices.Dtos;

// What a customer sees when choosing a service: no part breakdown, just duration and price.
public sealed record BookableServiceDto(
    Guid Id,
    string Name,
    int EstimatedDurationInMins,
    decimal LaborCost,
    decimal EstimatedTotal);
