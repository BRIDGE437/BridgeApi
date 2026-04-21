using MatchingApi.Data;
using MatchingApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Load .env from root ──
var rootPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".."));
var envPath = Path.Combine(rootPath, ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var parts = line.Split('=', 2);
        if (parts.Length != 2) continue;
        var key = parts[0].Trim();
        var value = parts[1].Trim().Trim('"').Trim('\'');
        Environment.SetEnvironmentVariable(key, value);
    }
}

// ── Database ──
var connectionString = Environment.GetEnvironmentVariable("PGVECTOR_DATABASE_URL") 
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Cleanup for Neon URI format (Npgsql fix)
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

        // Reconstruct in standard Npgsql format (more robust key-value pairs)
        connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SslMode=Require;Trust Server Certificate=true";
        
        // Debug logging (masking password)
        var maskedString = connectionString.Replace(password, "******");
        Console.WriteLine($"[DB MIGRATION] Using Connection String: {maskedString}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Error parsing connection URI: {ex.Message}");
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        o => o.UseVector()));

// ── Services ──
builder.Services.AddScoped<RuleBasedMatchingService>();
builder.Services.AddScoped<AiMatchingService>();
builder.Services.AddScoped<CsvImportService>();
builder.Services.AddScoped<StartupSimilarityService>();

// ── Background Workers ──
builder.Services.AddHostedService<StartupIndexingWorker>();
builder.Services.AddHostedService<EventMatchingWorker>();

// ── HTTP Client for AI microservice ──
builder.Services.AddHttpClient("AiService", client =>
{
    var baseUrl = builder.Configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Controllers + Swagger ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Startup-Investor Matching API", Version = "v1" });
});

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Frontend:Url"] ?? "http://localhost:3000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Migration on startup ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware ──
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
