using BuddyAssist.API.Data;
using BuddyAssist.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BuddyAssist.API.Endpoints
{
    public static class AdEndpoints
    {
        public static void MapAdEndpoints(this WebApplication app)
        {
            app.MapGet("/api/ads", async (AppDbContext db) =>
            {
                var now = DateTime.UtcNow;
                var ads = await db.Advertisements
                    .Where(a => a.IsActive && a.ExpiresAt > now)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
                return Results.Ok(ads);
            });

            app.MapGet("/api/ads/all", async (AppDbContext db) =>
            {
                var ads = await db.Advertisements
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
                return Results.Ok(ads);
            }).RequireAuthorization("AdminOnly");

            app.MapPost("/api/ads", async (Advertisement ad, AppDbContext db) =>
            {
                ad.CreatedAt = DateTime.UtcNow;
                ad.IsActive = true;
                ad.Clicks = 0;
                db.Advertisements.Add(ad);
                await db.SaveChangesAsync();
                return Results.Created($"/api/ads/{ad.Id}", ad);
            }).RequireAuthorization("AdminOnly");

            app.MapPost("/api/ads/{id}/click", async (int id, AppDbContext db) =>
            {
                var ad = await db.Advertisements.FindAsync(id);
                if (ad is null) return Results.NotFound();
                ad.Clicks += 1;
                await db.SaveChangesAsync();
                return Results.Ok(new { clicks = ad.Clicks });
            });

            app.MapPut("/api/ads/{id}", async (int id, Advertisement updated, AppDbContext db) =>
            {
                var ad = await db.Advertisements.FindAsync(id);
                if (ad is null) return Results.NotFound();
                ad.CompanyName = updated.CompanyName;
                ad.Description = updated.Description;
                ad.LogoUrl = updated.LogoUrl;
                ad.WebsiteUrl = updated.WebsiteUrl;
                ad.Category = updated.Category;
                ad.ContactEmail = updated.ContactEmail;
                ad.PhoneNumber = updated.PhoneNumber;
                ad.IsActive = updated.IsActive;
                ad.PricePerMonth = updated.PricePerMonth;
                ad.ExpiresAt = updated.ExpiresAt;
                await db.SaveChangesAsync();
                return Results.Ok(ad);
            }).RequireAuthorization("AdminOnly");

            app.MapDelete("/api/ads/{id}", async (int id, AppDbContext db) =>
            {
                var ad = await db.Advertisements.FindAsync(id);
                if (ad is null) return Results.NotFound();
                db.Advertisements.Remove(ad);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }).RequireAuthorization("AdminOnly");
        }
    }
}