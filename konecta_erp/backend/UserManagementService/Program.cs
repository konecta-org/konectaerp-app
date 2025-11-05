using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedContracts.ServiceDiscovery;
using Steeltoe.Extensions.Configuration.ConfigServer;
using UserManagementService.BackgroundServices;
using UserManagementService.Data;
using UserManagementService.Messaging;
using UserManagementService.Profiles;
using UserManagementService.Repositories;
using UserManagementService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddConfigServer(builder.Environment);
builder.Services.AddConsulServiceDiscovery(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Konecta ERP - User Management Service",
        Version = "v1",
        Description = "Provides user directory, role management, and synchronization endpoints."
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(UserMappingProfile).Assembly);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddHostedService<UserEventsConsumer>();

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

// app.UseHttpsRedirection();
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
        message = "Serving fallback response from User Management Service.",
        timestamp = DateTimeOffset.UtcNow
    }, statusCode: StatusCodes.Status503ServiceUnavailable));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
