using FinanceService.Data;
using FinanceService.Profiles;
using FinanceService.Repositories;
using FinanceService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedContracts.ServiceDiscovery;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddConsulServiceDiscovery(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Konecta ERP - Finance Service",
        Version = "v1",
        Description = "Finance domain APIs covering invoicing, expenses, budgets, payroll, and employee compensation management."
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(FinanceMappingProfile).Assembly);

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IFinanceSummaryService, FinanceSummaryService>();
builder.Services.AddScoped<IEmployeeCompensationRepository, EmployeeCompensationRepository>();
builder.Services.AddScoped<IEmployeeCompensationService, EmployeeCompensationService>();

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
        message = "Serving fallback response from Finance Service.",
        timestamp = DateTimeOffset.UtcNow
    }, statusCode: StatusCodes.Status503ServiceUnavailable));

app.Run();
