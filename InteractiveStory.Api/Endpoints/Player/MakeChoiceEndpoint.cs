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

public static class MakeChoiceEndpoint
{
    public static void MapMakeChoice(this RouteGroupBuilder group)
    {
        group.MapPost("/playthroughs/{playthroughId}/choices", async (Guid playthroughId, ChoiceRequest req, AppDbContext db, StoryCacheService cache) =>
        {
            var p = await db.Playthroughs.FirstOrDefaultAsync(x => x.Id == playthroughId);
            if (p == null) return Results.NotFound();
            if (p.Status != PlaythroughStatus.InProgress) return Results.BadRequest("Playthrough is already completed.");

            var def = await cache.GetStoryDefinitionAsync(p.StoryId);
            if (def == null || !def.Nodes.TryGetValue(p.CurrentNodeId, out var nodeDef))
                return Results.Problem("Story data error.");

            var choice = nodeDef.Choices.FirstOrDefault(c => c.Id == req.ChoiceId);
            if (choice == null) return Results.BadRequest("Invalid choice ID.");

            // Get next seq
            var maxSeq = await db.PlaythroughHistories
                .Where(h => h.PlaythroughId == p.Id)
                .MaxAsync(h => (int?)h.SequenceNumber) ?? 0;

            var history = new PlaythroughHistory
            {
                PlaythroughId = p.Id,
                NodeId = p.CurrentNodeId,
                ChosenChoiceId = choice.Id,
                SequenceNumber = maxSeq + 1,
                AnsweredAt = DateTime.UtcNow
            };
            db.PlaythroughHistories.Add(history);

            p.CurrentNodeId = choice.Next;
            
            // Check if next node is ending
            if (def.Nodes.TryGetValue(choice.Next, out var nextNodeDef))
            {
                if (nextNodeDef.IsEnding)
                {
                    p.Status = PlaythroughStatus.Completed;
                    p.CompletedAt = DateTime.UtcNow;
                }
            }
            else
            {
                return Results.Problem("Invalid next node configuration in story.");
            }

            await db.SaveChangesAsync();

            var safeChoices = nextNodeDef.Choices.Select(c => new
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
                    nextNodeDef.Text,
                    nextNodeDef.Media,
                    Choices = safeChoices,
                    nextNodeDef.IsEnding
                }
            });
        });
    }
}

public class ChoiceRequest
{
    public string ChoiceId { get; set; } = null!;
}
