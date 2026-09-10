using System;
using System.Linq;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Player;

public static class GetCurrentNodeEndpoint
{
    public static void MapGetCurrentNode(this RouteGroupBuilder group)
    {
        group.MapGet("/playthroughs/{playthroughId}/current", async (Guid playthroughId, AppDbContext db, StoryCacheService cache) =>
        {
            var p = await db.Playthroughs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == playthroughId);
            if (p == null) return Results.NotFound();

            var def = await cache.GetStoryDefinitionAsync(p.StoryId);
            if (def == null || !def.Nodes.TryGetValue(p.CurrentNodeId, out var nodeDef))
                return Results.Problem("Story data is missing or corrupted.");

            var safeChoices = nodeDef.Choices.Select(c => new
            {
                c.Id,
                c.Text,
                c.Media
            });

            return Results.Ok(new
            {
                Status = p.Status.ToString(),
                Node = new
                {
                    nodeDef.Text,
                    nodeDef.Media,
                    Choices = safeChoices,
                    nodeDef.IsEnding
                }
            });
        });
    }
}
