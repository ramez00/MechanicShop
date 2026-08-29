using MechanicShop.infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
        .AddApplicationLayer()
        .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
