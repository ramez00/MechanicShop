using Microsoft.AspNetCore.SignalR;

namespace MechanicShop.infrastructure.RealTime;

public class WorkOrderHub : Hub
{
    public const string HubURl = "/hubs/workorders";
}