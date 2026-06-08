using BuddyAssist.API.Data;
using BuddyAssist.API.Services;
using BuddyAssist.API.Hubs;
using BuddyAssist.API.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSignalR();

// ── DATABAS ───────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL") ?? "";

if (!string.IsNullOrEmpty(mysqlUrl))
{
    // Railway MySQL – konvertera URL till connection string
    var uri = new Uri(mysqlUrl);
    var userInfo = uri.UserInfo.Split(':');
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');
    var user = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    var mysqlConn = $"Server={host};Port={port};Database={database};User={user};Password={password};";
    Console.WriteLine($"🔗 Ansluter till MySQL: {host}:{port}/{database}");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn))
    );
}
else if (!string.IsNullOrEmpty(connectionString))
{
    // Lokal SQL Server
    Console.WriteLine("🔗 Ansluter till SQL Server (lokalt)");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString)
    );
}
else
{
    Console.WriteLine("⚠️ Ingen databas konfigurerad!");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer("")
    );
}

// ── CORS ──────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "https://buddyassist-frontend.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ── JWT ───────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"] ?? "FallbackKey_MinLength_32_Chars!!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BuddyAssist.API",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BuddyAssist.Frontend",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"))
    .AddPolicy("UserOnly", policy => policy.RequireRole("user", "admin"));

var app = builder.Build();

// ── MIGRATIONER ───────────────────────────────
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Console.WriteLine("✅ Migrationer kördes framgångsrikt!");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Migration misslyckades: {ex.Message}");
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ── SIGNALR ───────────────────────────────────
app.MapHub<ChatHub>("/hubs/chat");

// ── PING ──────────────────────────────────────
app.MapGet("/api/ping", () => "BuddyAssist API körs!");

// ── ENDPOINTS ─────────────────────────────────
app.MapAuthEndpoints();
app.MapMissionEndpoints();
app.MapUserEndpoints();
app.MapChatEndpoints();
app.MapNotificationEndpoints();
app.MapAdEndpoints();

app.Run();