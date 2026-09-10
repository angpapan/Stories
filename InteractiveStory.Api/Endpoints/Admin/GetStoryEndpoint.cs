using InteractiveStory.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class GetStoryEndpoint
{
    public static void MapGetStory(this RouteGroupBuilder group)
    {
        group.MapGet("/{id}", async (Guid id, AppDbContext db) =>
        {
            var story = await db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            return story != null ? Results.Ok(story) : Results.NotFound();
        });
    }
}
