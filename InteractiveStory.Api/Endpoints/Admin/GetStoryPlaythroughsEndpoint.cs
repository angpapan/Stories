using System.Linq;
using InteractiveStory.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class GetStoryPlaythroughsEndpoint
{
    public static void MapGetStoryPlaythroughs(this RouteGroupBuilder group)
    {
        group.MapGet("/{id}/playthroughs", async (Guid id, AppDbContext db) =>
        {
            var playthroughs = await db.Playthroughs
                .Where(p => p.StoryId == id)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var playthroughIds = playthroughs.Select(p => p.Id).ToList();

            var histories = await db.PlaythroughHistories
                .Where(h => playthroughIds.Contains(h.PlaythroughId))
                .OrderBy(h => h.SequenceNumber)
                .AsNoTracking()
                .ToListAsync();

            var result = playthroughs.Select(p => new
            {
                p.Id,
                p.Status,
                p.CreatedAt,
                p.CompletedAt,
                p.CurrentNodeId,
                History = histories
                    .Where(h => h.PlaythroughId == p.Id)
                    .Select(h => new
                    {
                        h.NodeId,
                        h.ChosenChoiceId,
                        h.AnsweredAt,
                        h.SequenceNumber
                    }).ToList()
            });

            return Results.Ok(result);
        });
    }
}
