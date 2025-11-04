using InventoryService.Data;
using InventoryService.Profiles;
using InventoryService.Repositories;
using InventoryService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedContracts.ServiceDiscovery;
using Steeltoe.Extensions.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddConfigServer(builder.Environment);
builder.Services.AddConsulServiceDiscovery(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Konecta ERP - Inventory Service",
        Version = "v1",
        Description = "Inventory management APIs for items, warehouses, stock transactions, and dashboards."
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(InventoryMappingProfile).Assembly);

builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IInventorySummaryService, InventorySummaryService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var serviceConfig = builder.Configuration.GetSection("ServiceConfig");
var serviceName = serviceConfig.GetValue<string>("ServiceName") ?? builder.Environment.ApplicationName;
var serviceHost = serviceConfig.GetValue<string>("Host") ?? "localhost";
var servicePort = serviceConfig.GetValue<int>("Port");
if (servicePort <= 0)
{
    throw new InvalidOperationException("ServiceConfig:Port must be a positive number.");
}

builder.WebHost.UseUrls($"http://{serviceHost}:{servicePort}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGet("/system/health", () =>
    Results.Ok(new
    {
        status = "UP",
        service = serviceName,
        timestamp = DateTimeOffset.UtcNow
    }));

app.MapGet("/system/fallback", () =>
    Results.Json(new
    {
        status = "UNAVAILABLE",
        service = serviceName,
        message = "Serving fallback response from Inventory Service.",
        timestamp = DateTimeOffset.UtcNow
    }, statusCode: StatusCodes.Status503ServiceUnavailable));

app.Run();
