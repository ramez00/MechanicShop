namespace MechanicShop.Client.Models;

public class BookableServiceModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int EstimatedDurationInMins { get; set; }
    public decimal LaborCost { get; set; }
    public decimal EstimatedTotal { get; set; }
}
