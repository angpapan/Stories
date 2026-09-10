using System;
using System.Linq;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Models;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Player;

public static class StartPlaythroughEndpoint
{
    public static void MapStartPlaythrough(this RouteGroupBuilder group)
    {
        group.MapPost("/stories/{storyId}/playthroughs", async (Guid storyId, StartPlaythroughRequest? req, AppDbContext db, StoryCacheService cache) =>
        {
            var def = await cache.GetStoryDefinitionAsync(storyId);
            if (def == null) return Results.NotFound("Story not found.");
            
            var story = await db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == storyId);
            if (story == null) return Results.NotFound("Story not found.");

            if (story.Password != null && story.Password != req?.Password)
                return Results.Unauthorized();

            if (story.MaxPlaythroughs.HasValue)
            {
                var count = await db.Playthroughs.CountAsync(p => p.StoryId == storyId);
                if (count >= story.MaxPlaythroughs.Value)
                    return Results.BadRequest("This story has reached its maximum allowed playthroughs and is locked.");
            }

            if (!def.Nodes.TryGetValue(def.StartNode, out var startNodeDef))
                return Results.BadRequest("Story start node is invalid.");

            var playthrough = new Playthrough
            {
                Id = Guid.NewGuid(),
                StoryId = storyId,
                CurrentNodeId = def.StartNode,
                Status = PlaythroughStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            db.Playthroughs.Add(playthrough);
            await db.SaveChangesAsync();

            var safeChoices = startNodeDef.Choices.Select(c => new
            {
                c.Id,
                c.Text,
                c.Media
            });

            return Results.Ok(new
            {
                PlaythroughId = playthrough.Id,
                Status = playthrough.Status.ToString(),
                Node = new
                {
                    startNodeDef.Text,
                    startNodeDef.Media,
                    Choices = safeChoices,
                    startNodeDef.IsEnding
                }
            });
        });
    }
}

public class StartPlaythroughRequest
{
    public string? Password { get; set; }
}
