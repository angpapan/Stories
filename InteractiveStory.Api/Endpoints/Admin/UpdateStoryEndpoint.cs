using System;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class UpdateStoryEndpoint
{
    public static void MapUpdateStory(this RouteGroupBuilder group)
    {
        group.MapPut("/{id}", async (Guid id, UpdateStoryRequest req, AppDbContext db, StoryCacheService cache) =>
        {
            var story = await db.Stories.FirstOrDefaultAsync(s => s.Id == id);
            if (story == null) return Results.NotFound();

            story.Title = req.Title;
            story.Description = req.Description;
            story.Json = req.Json;
            story.Password = req.Password;
            story.PasswordHint = req.PasswordHint;
            story.MaxPlaythroughs = req.MaxPlaythroughs;
            story.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            cache.InvalidateCache(id);

            return Results.Ok(story);
        });
    }
}

public class UpdateStoryRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Json { get; set; } = null!;
    public string? Password { get; set; }
    public string? PasswordHint { get; set; }
    public int? MaxPlaythroughs { get; set; }
}
