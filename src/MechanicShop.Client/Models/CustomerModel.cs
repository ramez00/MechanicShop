using System.Text.Json.Serialization;

namespace MechanicShop.Client.Models;

public class CustomerModel
{
    [JsonPropertyName("id")]
    public Guid CustomerId { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    [JsonPropertyName("cars")]
    public List<VehicleModel> Vehicles { get; set; } = [];
}