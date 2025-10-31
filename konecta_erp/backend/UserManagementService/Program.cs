using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserManagementService.Consumers;
using UserManagementService.Data;
using UserManagementService.Profiles;
using UserManagementService.Repositories;
using UserManagementService.Services;
using SharedContracts.ServiceDiscovery;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMassTransit(cfg =>
{
    cfg.AddConsumer<UserCreatedConsumer>();

    cfg.UsingRabbitMq((context, bus) =>
    {
        var rabbitSection = builder.Configuration.GetSection("RabbitMq");
        var hostName = rabbitSection["HostName"] ?? "localhost";
        var port = rabbitSection.GetValue("Port", 5672);
        var userName = rabbitSection["UserName"];
        var password = rabbitSection["Password"];
        var virtualHost = rabbitSection["VirtualHost"] ?? "/";
        var exchange = rabbitSection["Exchange"] ?? "konecta.erp";
        var queueName = rabbitSection["UserCreatedQueue"] ?? "user-management.user-created";
        var routingKey = rabbitSection["UserCreatedRoutingKey"] ?? "auth.user.created";

        bus.Host(hostName, (ushort)port, virtualHost, h =>
        {
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                h.Username(userName);
                h.Password(password);
            }
        });

        bus.ReceiveEndpoint(queueName, endpoint =>
        {
            endpoint.ConfigureConsumeTopology = false;
            endpoint.Bind(exchange, bind =>
            {
                bind.RoutingKey = routingKey;
                bind.ExchangeType = "topic";
            });
            endpoint.ConfigureConsumer<UserCreatedConsumer>(context);
        });
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

app.UseHttpsRedirection();
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
