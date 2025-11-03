using AuthenticationService.BackgroundServices;
using AuthenticationService.Data;
using AuthenticationService.Messaging;
using AuthenticationService.Models;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedContracts.ServiceDiscovery;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
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

    // Prefer HTTPS server to avoid auth header loss on HTTP->HTTPS redirects
    c.AddServer(new OpenApiServer { Url = "https://localhost:7280" });
    c.AddServer(new OpenApiServer { Url = "http://localhost:5099" });

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

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev
    options.IncludeErrorDetails = true;   // surface error in WWW-Authenticate
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT auth failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"JWT challenge: Error={context.Error}, Desc={context.ErrorDescription}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                Console.WriteLine("JWT message received with Authorization header.");
            }
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
        message = "Serving fallback response from Authentication Service.",
        timestamp = DateTimeOffset.UtcNow
    }, statusCode: StatusCodes.Status503ServiceUnavailable));

app.Run();
