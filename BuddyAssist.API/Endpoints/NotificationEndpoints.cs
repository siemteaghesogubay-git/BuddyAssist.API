using BuddyAssist.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BuddyAssist.API.Endpoints
{
    public static class NotificationEndpoints
    {
        public static void MapNotificationEndpoints(this WebApplication app)
        {
            app.MapGet("/api/notifications", async (
                HttpContext http, AppDbContext db) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();
                var userId = int.Parse(claim.Value);

                var notifications = await db.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(20)
                    .ToListAsync();

                return Results.Ok(notifications);
            }).RequireAuthorization("UserOnly");

            app.MapGet("/api/notifications/unread-count", async (
                HttpContext http, AppDbContext db) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();
                var userId = int.Parse(claim.Value);

                var count = await db.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                return Results.Ok(new { count });
            }).RequireAuthorization("UserOnly");

            app.MapPut("/api/notifications/{id}/read", async (
                int id, HttpContext http, AppDbContext db) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();

                var notification = await db.Notifications.FindAsync(id);
                if (notification is null) return Results.NotFound();

                notification.IsRead = true;
                await db.SaveChangesAsync();
                return Results.Ok(notification);
            }).RequireAuthorization("UserOnly");

            app.MapPut("/api/notifications/read-all", async (
                HttpContext http, AppDbContext db) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();
                var userId = int.Parse(claim.Value);

                var notifications = await db.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                notifications.ForEach(n => n.IsRead = true);
                await db.SaveChangesAsync();
                return Results.Ok();
            }).RequireAuthorization("UserOnly");
        }
    }
}