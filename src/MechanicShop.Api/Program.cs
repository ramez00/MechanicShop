using MechanicShop.infrastructure;
using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.RealTime;
using MechanicShop.infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddControllers();

builder.Services
        .AddApplicationLayer()
        .AddInfrastructure(builder.Configuration)
        .AddSignalR();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();

}

app.MapHub<WorkOrderHub>("/hubs/workorders");

app.MapControllers();

app.Run();
