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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

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

var jwtKey = builder.Configuration["Jwt:Key"] ?? "";

if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
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
}

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"))
    .AddPolicy("UserOnly", policy => policy.RequireRole("user", "admin"));

var app = builder.Build();

// Kör migrationer automatiskt vid startup
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

// OBS: Ta bort UseHttpsRedirection i produktion på Render
if (!app.Environment.IsProduction())
    app.UseHttpsRedirection();

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