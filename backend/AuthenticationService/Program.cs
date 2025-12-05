using AuthenticationService.BackgroundServices;
using AuthenticationService.Data;
using AuthenticationService.Messaging;
using AuthenticationService.Models;
using AuthenticationService.Options;
using AuthenticationService.Security;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using SharedContracts.Configuration;
using SharedContracts.ServiceDiscovery;
using Steeltoe.Extensions.Configuration.ConfigServer;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddConfigServer(builder.Environment);
builder.Services.AddConsulServiceDiscovery(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Konecta ERP - Authentication Service",
        Version = "v1",
        Description = "Authentication and Authorization API for Konecta ERP System"
    });

    // Default server for Swagger UI
    c.AddServer(new OpenApiServer { Url = "http://localhost:7280", Description = "Direct Access" });
    c.AddServer(new OpenApiServer { Url = "http://localhost:8080", Description = "API Gateway" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Paste only the token; Swagger prefixes it.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration - SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<ServiceAuthOptions>(builder.Configuration.GetSection(ServiceAuthOptions.SectionName));
builder.Services.AddSingleton<IJwksProvider, JwksProvider>();
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, AuthServiceJwtBearerOptionsSetup>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>().Configure(options =>
{
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                // Enforce Bearer scheme
                if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail("Authorization header must use Bearer scheme");
                    return Task.CompletedTask;
                }
                Console.WriteLine("JWT message received with valid Bearer token format.");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT auth failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"JWT challenge: Error={context.Error}, Desc={context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection(SendGridOptions.SectionName));
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<EmployeeEventsConsumer>();
builder.Services.AddHttpClient<IUserManagementClient, UserManagementClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ServiceAuthOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.UserManagementBaseUrl))
    {
        throw new InvalidOperationException("ServiceAuth:UserManagementBaseUrl must be configured.");
    }

    client.BaseAddress = new Uri(options.UserManagementBaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(10);
});

var serviceConfig = builder.Configuration.GetSection("ServiceConfig");
var serviceName = serviceConfig.GetValue<string>("ServiceName") ?? builder.Environment.ApplicationName;
var servicePort = serviceConfig.GetValue<int>("Port");
var serviceScheme = serviceConfig.GetValue<string>("Scheme") ?? "http";
if (servicePort <= 0)
{
    throw new InvalidOperationException("ServiceConfig:Port must be a positive number.");
}

if (string.Equals(serviceScheme, "https", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = servicePort;
    });
}

builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.ListenAnyIP(servicePort, listenOptions =>
    {
        if (string.Equals(serviceScheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            listenOptions.UseHttps();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (string.Equals(serviceScheme, "https", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Seed admin user after migrations
await AuthenticationService.Services.AdminSeeder.SeedAdminUser(app.Services);

app.MapGet("/system/health", () =>
    Results.Ok(new
    {
        status = "UP",
        service = serviceName,
        timestamp = DateTimeOffset.UtcNow
    })).AllowAnonymous();

app.MapGet("/system/fallback", () =>
    Results.Json(new
    {
        status = "UNAVAILABLE",
        service = serviceName,
        message = "Serving fallback response from Authentication Service.",
        timestamp = DateTimeOffset.UtcNow
    }, statusCode: StatusCodes.Status503ServiceUnavailable)).AllowAnonymous();

app.Run();
