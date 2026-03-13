using MatchingApi.Data;
using MatchingApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()));

// ── Services ──
builder.Services.AddScoped<RuleBasedMatchingService>();
builder.Services.AddScoped<AiMatchingService>();
builder.Services.AddScoped<CsvImportService>();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
