using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MechanicShop.infrastructure.RealTime;

[Authorize(Policy = "StaffOnly")]
public class WorkOrderHub : Hub
{
    public const string HubURl = "/hubs/workorders";
}
