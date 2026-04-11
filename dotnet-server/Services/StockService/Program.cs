using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using SharedDatabaseHelper;
using SharedDatabaseHelper.Settings;
using StockService.Consumers;
using StockService.Persistence;
using StockService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDatabase<StockServiceContext>(builder.Configuration);

builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<OrderCreatedConsumer>();
builder.Services.AddHostedService<PaymentFailedConsumer>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002, listenOptions =>
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

await DatabaseMigrationHelper.EnsureMigratedAsync<StockServiceContext>(
    app.Services,
    app.Services.GetRequiredService<ILogger<Program>>()
);
app.MapControllers();

app.Run();
