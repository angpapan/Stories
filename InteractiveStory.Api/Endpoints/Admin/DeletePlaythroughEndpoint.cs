using InteractiveStory.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class DeletePlaythroughEndpoint
{
    public static void MapDeletePlaythrough(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id}/playthroughs/{playthroughId}", async (Guid id, Guid playthroughId, AppDbContext db) =>
        {
            var playthrough = await db.Playthroughs.FirstOrDefaultAsync(p => p.Id == playthroughId && p.StoryId == id);
            if (playthrough == null) return Results.NoContent();

            db.Playthroughs.Remove(playthrough);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}
