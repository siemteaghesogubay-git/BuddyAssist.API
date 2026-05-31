using BuddyAssist.API.Data;
using BuddyAssist.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BuddyAssist.API.Endpoints
{
    public static class ChatEndpoints
    {
        public static void MapChatEndpoints(this WebApplication app)
        {
            app.MapGet("/api/chat/{otherUserId}", async (
                int otherUserId, HttpContext http, AppDbContext db) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();
                var myId = int.Parse(claim.Value);

                var messages = await db.ChatMessages
                    .Where(m =>
                        (m.SenderId == myId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == myId))
                    .OrderBy(m => m.SentAt)
                    .Select(m => new {
                        m.Id,
                        m.SenderId,
                        m.ReceiverId,
                        m.Message,
                        m.SentAt,
                        m.IsRead
                    })
                    .ToListAsync();

                return Results.Ok(messages);
            }).RequireAuthorization("UserOnly");

            app.MapPost("/api/chat", async (
                ChatMessageRequest req, HttpContext http, AppDbContext db) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();

                var msg = new ChatMessage
                {
                    SenderId = int.Parse(claim.Value),
                    ReceiverId = req.ReceiverId,
                    Message = req.Message,
                    SentAt = DateTime.UtcNow,
                };

                db.ChatMessages.Add(msg);
                await db.SaveChangesAsync();
                return Results.Ok(msg);
            }).RequireAuthorization("UserOnly");
        }
    }
}