using BuddyAssist.API.Data;
using BuddyAssist.API.Models;
using BuddyAssist.API.Services;
using BuddyAssist.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BuddyAssist.API.Endpoints
{
    public static class MissionEndpoints
    {
        public static void MapMissionEndpoints(this WebApplication app)
        {
            app.MapGet("/api/missions", async (AppDbContext db) =>
            {
                var missions = await db.Missions
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        m.Category,
                        m.Address,
                        m.ScheduledAt,
                        m.CreatedAt,
                        m.Status,
                        m.Points,
                        m.DistanceKm,
                        m.CreatedByUserId,
                        m.TakenByUserId,
                        m.HelperRating,
                        m.HelperComment,
                        m.CompletedAt,
                        TakenByName = db.Users
                            .Where(u => u.Id == m.TakenByUserId)
                            .Select(u => u.Name).FirstOrDefault(),
                        CreatedByName = db.Users
                            .Where(u => u.Id == m.CreatedByUserId)
                            .Select(u => u.Name).FirstOrDefault(),
                    })
                    .ToListAsync();
                return Results.Ok(missions);
            });

            app.MapGet("/api/missions/{id}", async (int id, AppDbContext db) =>
            {
                var mission = await db.Missions.FindAsync(id);
                return mission is null ? Results.NotFound() : Results.Ok(mission);
            });

            app.MapPost("/api/missions", async (
                Mission mission,
                HttpContext http,
                AppDbContext db,
                IHubContext<ChatHub> hubContext) =>
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out int uid))
                    mission.CreatedByUserId = uid;

                mission.Points = mission.Category switch
                {
                    "handla" => 50,
                    "transport" => 100,
                    "utbildning" => 80,
                    "sällskap" => 40,
                    "djurpassning" => 60,
                    _ => 50,
                };

                mission.CreatedAt = DateTime.UtcNow;
                mission.Status = "open";
                db.Missions.Add(mission);
                await db.SaveChangesAsync();

                var creatorName = await db.Users
                    .Where(u => u.Id == mission.CreatedByUserId)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync() ?? "Någon";

                var allUserIds = await db.Users
                    .Where(u => u.Id != mission.CreatedByUserId && !u.IsPaused)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var userId in allUserIds)
                {
                    db.Notifications.Add(new Notification
                    {
                        UserId = userId,
                        Title = "Nytt uppdrag nära dig! 📋",
                        Message = $"{creatorName} skapade: {mission.Title}",
                        Type = "mission",
                        RelatedId = mission.Id,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
                await db.SaveChangesAsync();

                await hubContext.Clients.All.SendAsync("NewMission", new
                {
                    title = mission.Title,
                    missionId = mission.Id,
                    creator = creatorName,
                    timestamp = DateTime.UtcNow,
                });

                return Results.Created($"/api/missions/{mission.Id}", mission);
            }).RequireAuthorization("UserOnly");

            app.MapPost("/api/missions/{id}/take", async (
                int id, HttpContext http, AppDbContext db, EmailService emailService) =>
            {
                var mission = await db.Missions.FindAsync(id);
                if (mission is null) return Results.NotFound();
                if (mission.Status != "open") return Results.BadRequest("Uppdraget är redan taget.");

                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                string takenByName = "Okänd";

                if (claim != null && int.TryParse(claim.Value, out int userId))
                {
                    mission.TakenByUserId = userId;
                    var helper = await db.Users.FindAsync(userId);
                    if (helper != null)
                    {
                        takenByName = helper.Name;
                        helper.TotalPoints += mission.Points;
                        helper.CompletedMissions += 1;
                        helper.CurrentLevel = helper.CompletedMissions switch
                        {
                            >= 100 => "legend",
                            >= 50 => "stjärna",
                            >= 30 => "diamant",
                            >= 15 => "guld",
                            >= 10 => "silver",
                            >= 5 => "brons",
                            _ => "ny"
                        };

                        try { await emailService.SendMissionTakenConfirmationAsync(helper.Email, helper.Name, mission.Title, mission.Points); }
                        catch (Exception ex) { Console.WriteLine($"Bekräftelsemejl: {ex.Message}"); }
                    }
                }

                mission.Status = "taken";
                await db.SaveChangesAsync();

                if (mission.CreatedByUserId > 0)
                {
                    var creator = await db.Users.FindAsync(mission.CreatedByUserId);
                    if (creator != null && !string.IsNullOrEmpty(creator.Email))
                    {
                        try { await emailService.SendMissionTakenNotificationAsync(creator.Email, creator.Name, takenByName, mission.Title); }
                        catch (Exception ex) { Console.WriteLine($"Notifieringsmejl: {ex.Message}"); }
                    }
                }

                return Results.Ok(mission);
            }).RequireAuthorization("UserOnly");

            app.MapPost("/api/missions/{id}/complete", async (
                int id, HttpContext http, AppDbContext db,
                EmailService emailService, CompleteRequest req) =>
            {
                var mission = await db.Missions.FindAsync(id);
                if (mission is null) return Results.NotFound();
                if (mission.Status != "taken") return Results.BadRequest("Uppdraget måste vara taget.");

                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null) return Results.Unauthorized();
                if (int.Parse(claim.Value) != mission.CreatedByUserId) return Results.Forbid();
                if (req.Rating < 1 || req.Rating > 5) return Results.BadRequest("Betyg 1-5.");

                mission.Status = "completed";
                mission.CompletedAt = DateTime.UtcNow;
                mission.HelperRating = req.Rating;
                mission.HelperComment = req.Comment;

                if (mission.TakenByUserId.HasValue)
                {
                    var helper = await db.Users.FindAsync(mission.TakenByUserId.Value);
                    if (helper != null)
                    {
                        var ratings = await db.Missions
                            .Where(m => m.TakenByUserId == helper.Id && m.HelperRating.HasValue)
                            .Select(m => m.HelperRating!.Value)
                            .ToListAsync();
                        ratings.Add(req.Rating);
                        helper.Rating = Math.Round(ratings.Average(), 1);

                        try { await emailService.SendMissionCompletedEmailAsync(helper.Email, helper.Name, mission.Title, req.Rating, req.Comment ?? ""); }
                        catch (Exception ex) { Console.WriteLine($"Tack-mejl: {ex.Message}"); }
                    }
                }

                await db.SaveChangesAsync();
                return Results.Ok(mission);
            }).RequireAuthorization("UserOnly");

            app.MapPut("/api/missions/{id}/pause", async (int id, AppDbContext db) =>
            {
                var m = await db.Missions.FindAsync(id);
                if (m is null) return Results.NotFound();
                m.Status = "paused";
                await db.SaveChangesAsync();
                return Results.Ok(m);
            }).RequireAuthorization("AdminOnly");

            app.MapPut("/api/missions/{id}/activate", async (int id, AppDbContext db) =>
            {
                var m = await db.Missions.FindAsync(id);
                if (m is null) return Results.NotFound();
                m.Status = "open";
                await db.SaveChangesAsync();
                return Results.Ok(m);
            }).RequireAuthorization("AdminOnly");

            app.MapDelete("/api/missions/{id}", async (int id, AppDbContext db) =>
            {
                var m = await db.Missions.FindAsync(id);
                if (m is null) return Results.NotFound();
                db.Missions.Remove(m);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization("AdminOnly");
        }
    }
}