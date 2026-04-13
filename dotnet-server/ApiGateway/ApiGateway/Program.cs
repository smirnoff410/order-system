using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 1. Загружаем конфигурацию маршрутов из appsettings.json
var proxyConfig = builder.Configuration.GetSection("ReverseProxy");

// 2. Добавляем Reverse Proxy (YARP)
builder.Services.AddReverseProxy()
    .LoadFromConfig(proxyConfig);

// 3. Добавляем CORS для React приложения
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()  // React приложение
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Добавляем Swagger для документации API Gateway
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Gateway", Version = "v1" });
});

var app = builder.Build();

// 5. Настройка CORS
app.UseCors("ReactApp");

// 6. Swagger (только для разработки)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 7. Логирование запросов (опционально)
app.Use(async (context, next) =>
{
    Console.WriteLine($"[API Gateway] {context.Request.Method} {context.Request.Path}");
    await next();
});

// 8. Middleware для обработки ошибок
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"API Gateway error\"}");
    });
});

// 9. Запускаем Reverse Proxy
app.MapReverseProxy();

// 10. Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();