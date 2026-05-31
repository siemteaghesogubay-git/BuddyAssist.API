using BuddyAssist.API.Data;
using BuddyAssist.API.Models;
using BuddyAssist.API.Services;
using BuddyAssist.API.Helpers;
using Microsoft.EntityFrameworkCore;


namespace BuddyAssist.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            app.MapPost("/api/auth/register", async (
                RegisterRequest req, AppDbContext db, EmailService emailService) =>
            {
                var exists = await db.Users.AnyAsync(u => u.Email == req.Email);
                if (exists) return Results.BadRequest("Email används redan.");

                var isFirst = !await db.Users.AnyAsync();

                var user = new User
                {
                    Name = req.Name,
                    Email = req.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                    City = req.City,
                    Role = isFirst ? "admin" : "user",
                    IsPaused = false,
                    CurrentLevel = "brons",
                    JoinedAt = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                try { await emailService.SendWelcomeEmailAsync(user.Email, user.Name); }
                catch (Exception ex) { Console.WriteLine($"Välkomstmejl misslyckades: {ex.Message}"); }

                var token = TokenHelper.GenerateToken(user, app.Configuration);
                return Results.Ok(new AuthResponse
                {
                    Token = token,
                    Name = user.Name,
                    UserId = user.Id,
                    Role = user.Role
                });
            });

            app.MapPost("/api/auth/login", async (LoginRequest req, AppDbContext db) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
                if (user is null) return Results.Unauthorized();
                if (user.IsPaused) return Results.BadRequest("Ditt konto är pausat. Kontakta admin.");

                var valid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
                if (!valid) return Results.Unauthorized();

                var token = TokenHelper.GenerateToken(user, app.Configuration);
                return Results.Ok(new AuthResponse
                {
                    Token = token,
                    Name = user.Name,
                    UserId = user.Id,
                    Role = user.Role
                });
            });
        }
    }
}