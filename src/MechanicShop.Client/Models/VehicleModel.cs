using System.Text.Json.Serialization;

namespace MechanicShop.Client.Models;

public class VehicleModel
{
    [JsonPropertyName("id")]
    public Guid VehicleId { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string LicensePlate { get; set; } = string.Empty;

}