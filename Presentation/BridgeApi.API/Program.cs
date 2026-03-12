using System.Text;
using BridgeApi.Application;
using BridgeApi.API.Configurations;
using BridgeApi.API.Middlewares;
using BridgeApi.Domain.Entities;
using BridgeApi.Infrastructure;
using BridgeApi.Persistence;
using BridgeApi.Persistence.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddStorage(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);
builder.Services.AddControllers();

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
});

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
