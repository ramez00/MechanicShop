using FluentValidation;
using MechanicShop.Application.Common.Interfaces;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetAvailability;

public sealed class GetAvailabilityQueryValidator : AbstractValidator<GetAvailabilityQuery>
{
    public GetAvailabilityQueryValidator(TimeProvider timeProvider, IShopSettings settings)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        RuleFor(q => q.RepairTaskIds)
            .NotEmpty().WithMessage("At least one repair task must be selected.")
            .Must(ids => ids.Count <= 10).WithMessage("A maximum of 10 services can be selected.");

        RuleFor(q => q.Date)
            .Must(d => d >= today)
            .WithMessage("Date cannot be in the past.")
            .Must(d => d <= today.AddDays(settings.BookingHorizonDays))
            .WithMessage($"Date cannot be more than {settings.BookingHorizonDays} days in the future.");
    }
}
