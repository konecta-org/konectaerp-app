using HrService.BackgroundServices;
using HrService.Data;
using HrService.Messaging;
using HrService.Profiles;
using HrService.Repositories;
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
        Title = "Konecta ERP - HR Service",
        Version = "v1",
        Description = "Manages departments and employees."
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(HrMappingProfile).Assembly);

builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
builder.Services.AddScoped<IDepartmentRepo, DepartmentRepo>();
builder.Services.AddScoped<IJobOpeningRepo, JobOpeningRepo>();
builder.Services.AddScoped<IJobApplicationRepo, JobApplicationRepo>();
builder.Services.AddScoped<IInterviewRepo, InterviewRepo>();
builder.Services.AddScoped<ILeaveRequestRepo, LeaveRequestRepo>();
builder.Services.AddScoped<IAttendanceRepo, AttendanceRepo>();
builder.Services.AddScoped<IResignationRequestRepo, ResignationRequestRepo>();

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<UserProvisionedConsumer>();

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
        message = "Serving fallback response from HR Service.",
        timestamp = DateTimeOffset.UtcNow
    }, statusCode: StatusCodes.Status503ServiceUnavailable));

app.Run();
