using System;
using System.Linq;
using System.Threading.Tasks;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Models;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints;

public static class PlayerEndpoints
{
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        group.MapPost("/stories/{storyId}/playthroughs", async (string storyId, StartPlaythroughRequest? req, AppDbContext db, StoryCacheService cache) =>
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
                Node = new
                {
                    startNodeDef.Text,
                    startNodeDef.Media,
                    Choices = safeChoices,
                    startNodeDef.IsEnding
                }
            });
        });

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
                p.Status,
                Node = new
                {
                    nodeDef.Text,
                    nodeDef.Media,
                    Choices = safeChoices,
                    nodeDef.IsEnding
                }
            });
        });

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
                p.Status,
                Node = new
                {
                    nextNodeDef.Text,
                    nextNodeDef.Media,
                    Choices = safeChoices,
                    nextNodeDef.IsEnding
                }
            });
        });

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

public class ChoiceRequest
{
    public string ChoiceId { get; set; } = null!;
}

public class StartPlaythroughRequest
{
    public string? Password { get; set; }
}
