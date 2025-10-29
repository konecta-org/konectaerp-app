using InventoryService.Data;
using InventoryService.Profiles;
using InventoryService.Repositories;
using InventoryService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
