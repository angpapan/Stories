using System;
using System.Linq;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Player;

public static class GetPlaythroughHistoryEndpoint
{
    public static void MapGetPlaythroughHistory(this RouteGroupBuilder group)
    {
        group.MapGet("/playthroughs/{playthroughId}/history", async (Guid playthroughId, AppDbContext db, StoryCacheService cache) =>
        {
            var p = await db.Playthroughs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == playthroughId);
            if (p == null) return Results.NotFound();

            var history = await db.PlaythroughHistories
                .AsNoTracking()
                .Where(h => h.PlaythroughId == playthroughId)
                .OrderBy(h => h.SequenceNumber)
                .ToListAsync();

            var def = await cache.GetStoryDefinitionAsync(p.StoryId);

            var recap = history.Select(h => {
                string? nodeText = null;
                string? choiceText = null;

                if (def != null && def.Nodes.TryGetValue(h.NodeId, out var node))
                {
                    nodeText = node.Text;
                    if (h.ChosenChoiceId != null)
                    {
                        choiceText = node.Choices.FirstOrDefault(c => c.Id == h.ChosenChoiceId)?.Text;
                    }
                }

                return new {
                    h.SequenceNumber,
                    h.NodeId,
                    NodeText = nodeText,
                    h.ChosenChoiceId,
                    ChoiceText = choiceText,
                    h.AnsweredAt
                };
            });

            return Results.Ok(recap);
        });
    }
}
