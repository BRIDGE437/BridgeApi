using System.Text;
using BridgeApi.Application;
using BridgeApi.API.Configurations;
using BridgeApi.API.Middlewares;
using BridgeApi.Domain.Entities;
using BridgeApi.Infrastructure;
using BridgeApi.Persistence;
using BridgeApi.RealtimeCommunication;
using BridgeApi.RealtimeCommunication.Hubs;
using BridgeApi.Persistence.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// ── Load .env from root (Aggressive Discovery) ──
string[] searchPaths = { builder.Environment.ContentRootPath, Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
bool envLoaded = false;

foreach (var path in searchPaths)
{
    var dir = new DirectoryInfo(path);
    while (dir != null && !envLoaded)
    {
        var potentialPath = Path.Combine(dir.FullName, ".env");
        if (File.Exists(potentialPath))
        {
            foreach (var line in File.ReadAllLines(potentialPath))
            {
                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;
                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"').Trim('\'');
                Environment.SetEnvironmentVariable(key, value);
            }
            envLoaded = true;
            Console.WriteLine($"[CONFIG] .env loaded from: {potentialPath}");
        }
        dir = dir.Parent;
    }
}

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Database Connection from Environment or Config
var connectionString = Environment.GetEnvironmentVariable("PGVECTOR_DATABASE_URL") 
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

// ── Fix: Parse postgresql:// URI for Npgsql ──
if (connectionString != null && (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://")))
{
    try 
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var user = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        // Standard Npgsql format
        connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SslMode=Require;Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Error parsing DB URI: {ex.Message}");
    }
}

if (connectionString != null && connectionString.Contains("neon.tech"))
    Console.WriteLine("[DB] Using Cloud Database (Neon)");
else
    Console.WriteLine("[DB] Using Local Database (127.0.0.1)");

builder.Services.AddPersistence(builder.Configuration, connectionString);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddStorage(builder.Configuration);
builder.Services.AddRealtimeCommunication(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = 25 * 1024 * 1024);

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecurityKey"]!))
    };

    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
                
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    string[] roles = ["Founder", "Investor", "Talent", "Admin"];

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new AppRole { Id = Guid.NewGuid().ToString(), Name = role });
    }
}

// Seed development data
if (app.Environment.IsDevelopment())
{
    await DatabaseSeeder.SeedAsync(app.Services);
}

if (app.Environment.IsDevelopment() && args.Contains("--clear-seed"))
{
    await DatabaseSeeder.ClearAsync(app.Services);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

if (builder.Configuration.GetValue("RateLimiting:Enabled", true))
    app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<PresenceHub>("/hubs/presence");

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
