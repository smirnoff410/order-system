using Confluent.Kafka;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OrderService.Consumers;
using OrderService.Persistence;
using OrderService.Services;
using Serilog;
using SharedDatabaseHelper;
using SharedDatabaseHelper.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration) // Read from appsettings.json
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddDatabase<OrderServiceContext>(builder.Configuration);

builder.Services.AddSingleton<KafkaProducerService>();

builder.Services.AddHostedService<PaymentCompletedConsumer>();
builder.Services.AddHostedService<PaymentFailedConsumer>();
builder.Services.AddHostedService<StockFailedConsumer>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await DatabaseMigrationHelper.EnsureMigratedAsync<OrderServiceContext>(
    app.Services,
    app.Services.GetRequiredService<ILogger<Program>>()
);

app.UseSerilogRequestLogging();
app.MapControllers();
app.UseCors("AllowReactApp");
app.MapHealthChecks("/health");
app.Run();
