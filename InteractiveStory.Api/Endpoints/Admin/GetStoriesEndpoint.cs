using System.Linq;
using InteractiveStory.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class GetStoriesEndpoint
{
    public static void MapGetStories(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (AppDbContext db) =>
        {
            var stories = await db.Stories
                .AsNoTracking()
                .Select(s => new { s.Id, s.Title, s.Description, s.Password, s.PasswordHint, s.MaxPlaythroughs, s.UpdatedAt })
                .ToListAsync();
            return Results.Ok(stories);
        });
    }
}
