using InteractiveStory.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Player;

public static class GetStoryInfoEndpoint
{
    public static void MapGetStoryInfo(this RouteGroupBuilder group)
    {
        group.MapGet("/stories/{storyId}", async (Guid storyId, AppDbContext db) =>
        {
            var story = await db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == storyId);
            if (story == null) return Results.NotFound("Story not found.");

            var requiresPassword = !string.IsNullOrEmpty(story.Password);

            return Results.Ok(new
            {
                Title = requiresPassword ? null : story.Title,
                Description = requiresPassword ? null : story.Description,
                RequiresPassword = requiresPassword,
                PasswordHint = requiresPassword ? story.PasswordHint : null
            });
        });

        group.MapPost("/stories/{storyId}/unlock", async (Guid storyId, UnlockRequest req, AppDbContext db) =>
        {
            var story = await db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == storyId);
            if (story == null) return Results.NotFound("Story not found.");

            if (!string.IsNullOrEmpty(story.Password) && story.Password != req.Password)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                Title = story.Title,
                Description = story.Description,
                RequiresPassword = !string.IsNullOrEmpty(story.Password)
            });
        });
    }
}

public class UnlockRequest
{
    public string Password { get; set; } = string.Empty;
}
