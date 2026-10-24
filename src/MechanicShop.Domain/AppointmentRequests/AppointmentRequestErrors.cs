using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.AppointmentRequests;

public static class AppointmentRequestErrors
{
    public static Error NotPending => Error.Conflict(
        "AppointmentRequest_NotPending",
        "This action requires the appointment request to be in Pending status.");

    public static Error NotApproved => Error.Conflict(
        "AppointmentRequest_NotApproved",
        "This action requires the appointment request to be in Approved status.");

    public static Error RepairTasksRequired => Error.Validation(
        "AppointmentRequest_RepairTasksRequired",
        "At least one repair task is required.");

    public static Error InvalidTiming => Error.Validation(
        "AppointmentRequest_InvalidTiming",
        "The requested end time must be after the start time.");

    public static Error NotesTooLong => Error.Validation(
        "AppointmentRequest_NotesTooLong",
        "Notes cannot exceed 500 characters.");

    public static Error CustomerIdRequired => Error.Validation(
        "AppointmentRequest_CustomerIdRequired",
        "Customer ID is required.");

    public static Error CarIdRequired => Error.Validation(
        "AppointmentRequest_CarIdRequired",
        "Car ID is required.");
}
