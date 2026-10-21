using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Appointments;

public class SubmitAppointmentRequestRequest
{
    [Required]
    public Guid CarId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one repair task must be selected.")]
    public List<Guid> RepairTaskIds { get; set; } = [];

    [Required]
    public DateTimeOffset RequestedStartAt { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string? Notes { get; set; }
}
