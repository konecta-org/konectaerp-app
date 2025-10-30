using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserManagementService.Consumers;
using UserManagementService.Data;
using UserManagementService.Profiles;
using UserManagementService.Repositories;
using UserManagementService.Services;

var builder = WebApplication.CreateBuilder(args);

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
