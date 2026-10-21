using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Appointments;

public class RejectAppointmentRequestRequest
{
    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string Reason { get; set; } = string.Empty;
}
