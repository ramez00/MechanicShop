using System.ComponentModel.DataAnnotations;
using MecanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.Appointments;

public class ApproveAppointmentRequestRequest
{
    [Required(ErrorMessage = "Labor is required.")]
    public Guid LaborId { get; set; }

    [Required(ErrorMessage = "Spot is required.")]
    [Range(0, 3, ErrorMessage = "Invalid range [0, 1, 2 or 3]")]
    public Spot Spot { get; set; }
}
