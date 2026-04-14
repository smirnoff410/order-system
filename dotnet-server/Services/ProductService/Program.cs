using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProductService.Persistence;
using SharedDatabaseHelper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddDatabase<ProductServiceContext>(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5005, listenOptions =>
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
await DatabaseMigrationHelper.EnsureMigratedAsync<ProductServiceContext>(
    app.Services,
    app.Services.GetRequiredService<ILogger<Program>>()
);

app.MapControllers();
app.UseCors("AllowReactApp");
app.MapHealthChecks("/health");
app.Run();
