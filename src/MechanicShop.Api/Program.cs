using MechanicShop.infrastructure;
using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.RealTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services
        .AddApplicationLayer()
        .AddInfrastructure(builder.Configuration)
        .AddSignalR();


var app = builder.Build();

// app.MapGet("/", () => "Hello World!");
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();

}

app.MapHub<WorkOrderHub>("/hubs/workorders");

app.Run();
