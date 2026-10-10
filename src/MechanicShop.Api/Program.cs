using Asp.Versioning;
using MechanicShop.Api.Handlers;
using MechanicShop.infrastructure;
using MechanicShop.infrastructure.Data;
using MechanicShop.infrastructure.RealTime;
using MechanicShop.infrastructure.Settings;
using MechanicShop.Api.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplicationLayer()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();

}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>().AllowAnonymous()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MechanicShop.Client._Imports).Assembly);

app.MapHub<WorkOrderHub>("/hubs/workorders");

app.MapControllers();

app.Run();
