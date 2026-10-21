using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Errors;

public static class ApplicationErrors
{
    public static Error WorkOrderOutsideOperatingHour(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) =>
    Error.Conflict(
           "ApplicationErrors.WorkOrder.Outside.OperatingHours",
           $"The WorkOrder time ({startAtUtc} ? {endAtUtc}) is outside of store operating hours.");

    public static Error WorkOrderNotFound => Error.NotFound(
           "ApplicationErrors.WorkOrder.NotFound",
           "WorkOrder does not exist.");

    public static Error LaborOccupied =>
    Error.Conflict(
           "Employee.LaborOccupied",
           "Labor is already occupied during the requested time.");

    public static Error CustomerNotFound =>
    Error.NotFound(
           "ApplicationErrors.Customer.NotFound",
           "Customer does not exist.");

    public static Error VehicleNotFound =>
    Error.NotFound(
           "ApplicationErrors.Vehicle.NotFound",
           "Vehicle does not exist.");

    public static Error VehicleSchedulingConflict =>
    Error.Conflict(
            "Vehicle_Overlapping_WorkOrder",
            "The vehicle already has an overlapping WorkOrder.");

    public static Error RepairTaskNotFound =>
    Error.NotFound(
            "RepairTask.NotFound",
            "Repair task does not exist.");

    public static Error WorkOrderMustBeCompletedForInvoicing =>
    Error.Conflict(
            "WorkOrder.InvoiceIssuance.InvalidState",
            "WorkOrder must be in 'Completed' state to issue an invoice.");

    public static Error WorkOrderAlreadyInvoiced =>
    Error.Conflict(
            "WorkOrder.InvoiceIssuance.AlreadyInvoiced",
            "An invoice has already been issued for this work order.");

    public static Error InvoiceNotFound => Error.NotFound(
       "ApplicationErrors.Invoice.NotFound",
       "Invoice does not exist.");

    public static Error InvalidRefreshToken =>
    Error.Validation(
        "RefreshToken.Expiry.Invalid",
        "Expiry must be in the future.");

    public static readonly Error ExpiredAccessTokenInvalid = Error.Conflict(
         code: "Auth.ExpiredAccessToken.Invalid",
         description: "Expired access token is not valid.");

    public static readonly Error UserIdClaimInvalid = Error.Conflict(
        code: "Auth.UserIdClaim.Invalid",
        description: "Invalid userId claim.");

    public static readonly Error RefreshTokenExpired = Error.Conflict(
        code: "Auth.RefreshToken.Expired",
        description: "Refresh token is invalid or has expired.");

    public static readonly Error UserNotFound = Error.NotFound(
        code: "Auth.User.NotFound",
        description: "User not found.");

    public static Error UserNameTaken => Error.Conflict(
        code: "User_UserNameTaken",
        description: "This username is already taken.");

    public static Error EmailTaken => Error.Conflict(
        code: "User_EmailTaken",
        description: "An account with this email already exists.");

    // Unknown user, wrong password and locked-out account all return this, so none can be told apart.
    public static Error InvalidCredentials => Error.Unauthorized(
        code: "Auth_InvalidCredentials",
        description: "Username or password is incorrect.");

    public static Error CustomerAccountNotFound => Error.Unauthorized(
        code: "Auth_CustomerNotFound",
        description: "No customer profile is linked to this account.");

    public static Error EmailNotConfirmed => Error.Forbidden(
        code: "User_EmailNotConfirmed",
        description: "Please confirm your email address before signing in.");

    // Deliberately generic: unknown user, tampered token and expired token all look the same to the caller.
    public static Error InvalidEmailConfirmationLink => Error.Validation(
        code: "User_InvalidConfirmationLink",
        description: "This confirmation link is invalid or has expired.");

    public static readonly Error TokenGenerationFailed = Error.Failure(
        code: "Auth.TokenGeneration.Failed",
        description: "Failed to generate new JWT token.");

    public static Error LaborNotFound =>
        Error.NotFound("Employee.LaborNotFound", "Labor does not exist.");

    public static Error AppointmentSlotNoLongerAvailable =>
        Error.Conflict("Appointment_SlotNoLongerAvailable",
            "The requested time slot is no longer available.");

    public static Error AppointmentCarNotOwned =>
        Error.NotFound("Appointment_CarNotOwned",
            "The selected vehicle does not belong to you.");
}