namespace MechanicShop.Application.Features.RepairTasks;

// Every cached query that exposes repair-task data carries this tag; every write evicts it.
public static class RepairTaskCacheTags
{
    public const string RepairTask = "repair-task";
}
