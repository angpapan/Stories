using InteractiveStory.Api.Data;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class DeleteStoryEndpoint
{
    public static void MapDeleteStory(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id}", async (Guid id, AppDbContext db, StoryCacheService cache) =>
        {
            var story = await db.Stories.FirstOrDefaultAsync(s => s.Id == id);
            if (story == null) return Results.NoContent();

            var hasPlaythroughs = await db.Playthroughs.AnyAsync(p => p.StoryId == id);
            if (hasPlaythroughs)
            {
                return Results.BadRequest("Cannot delete story because it has playthroughs.");
            }

            db.Stories.Remove(story);
            await db.SaveChangesAsync();
            cache.InvalidateCache(id);

            return Results.NoContent();
        });
    }
}
