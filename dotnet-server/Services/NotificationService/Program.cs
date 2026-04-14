using Microsoft.AspNetCore.Server.Kestrel.Core;
using NotificationService.Consumers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration) // Read from appsettings.json
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddHostedService<OrderCompletedConsumer>();
builder.Services.AddHostedService<OrderFailedConsumer>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5004, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");

app.Run();
