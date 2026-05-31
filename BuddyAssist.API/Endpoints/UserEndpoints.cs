using BuddyAssist.API.Data;
using BuddyAssist.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BuddyAssist.API.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            app.MapGet("/api/users", async (AppDbContext db) =>
            {
                var users = await db.Users
                    .Select(u => new {
                        u.Id,
                        u.Name,
                        u.City,
                        u.TotalPoints,
                        u.CompletedMissions,
                        u.CurrentLevel,
                        u.Rating,
                        u.Role,
                        u.IsPaused,
                        u.JoinedAt,
                        u.ProfileImage
                    })
                    .OrderByDescending(u => u.TotalPoints)
                    .ToListAsync();
                return Results.Ok(users);
            });

            app.MapPut("/api/users/profile-image", async (
                HttpContext http, AppDbContext db, ProfileImageRequest req) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();
                var userId = int.Parse(claim.Value);

                var user = await db.Users.FindAsync(userId);
                if (user is null) return Results.NotFound();

                if (req.ImageBase64.Length > 3 * 1024 * 1024)
                    return Results.BadRequest("Bilden är för stor. Max 2MB.");

                user.ProfileImage = req.ImageBase64;
                await db.SaveChangesAsync();
                return Results.Ok(new { profileImage = user.ProfileImage });
            }).RequireAuthorization("UserOnly");

            app.MapGet("/api/users/{id}/profile-image", async (int id, AppDbContext db) =>
            {
                var user = await db.Users.FindAsync(id);
                if (user is null) return Results.NotFound();
                return Results.Ok(new { profileImage = user.ProfileImage });
            });

            app.MapPut("/api/users/{id}/pause", async (int id, AppDbContext db) =>
            {
                var u = await db.Users.FindAsync(id);
                if (u is null) return Results.NotFound();
                u.IsPaused = true;
                await db.SaveChangesAsync();
                return Results.Ok(u);
            }).RequireAuthorization("AdminOnly");

            app.MapPut("/api/users/{id}/activate", async (int id, AppDbContext db) =>
            {
                var u = await db.Users.FindAsync(id);
                if (u is null) return Results.NotFound();
                u.IsPaused = false;
                await db.SaveChangesAsync();
                return Results.Ok(u);
            }).RequireAuthorization("AdminOnly");

            app.MapDelete("/api/users/{id}", async (int id, AppDbContext db) =>
            {
                var u = await db.Users.FindAsync(id);
                if (u is null) return Results.NotFound();
                db.Users.Remove(u);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization("AdminOnly");

            app.MapPut("/api/users/{id}/role", async (int id, string role, AppDbContext db) =>
            {
                var u = await db.Users.FindAsync(id);
                if (u is null) return Results.NotFound();
                u.Role = role;
                await db.SaveChangesAsync();
                return Results.Ok(u);
            }).RequireAuthorization("AdminOnly");

            app.MapPut("/api/users/{id}/edit", async (
                int id, AppDbContext db, EditUserRequest req) =>
            {
                var user = await db.Users.FindAsync(id);
                if (user is null) return Results.NotFound();

                if (!string.IsNullOrWhiteSpace(req.Name)) user.Name = req.Name;
                if (!string.IsNullOrWhiteSpace(req.City)) user.City = req.City;
                if (!string.IsNullOrWhiteSpace(req.Role)) user.Role = req.Role;
                if (req.ClearProfileImage) user.ProfileImage = null;

                await db.SaveChangesAsync();
                return Results.Ok(user);
            }).RequireAuthorization("AdminOnly");
        }
    }
}