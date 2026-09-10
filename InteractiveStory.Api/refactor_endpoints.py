import os

base_dir = "/home/angpap/repos/Stories/InteractiveStory.Api/Endpoints"
admin_dir = os.path.join(base_dir, "Admin")
player_dir = os.path.join(base_dir, "Player")

os.makedirs(admin_dir, exist_ok=True)
os.makedirs(player_dir, exist_ok=True)

admin_get_stories = """using System.Linq;
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
                .Select(s => new { s.Id, s.Title, s.Description, s.UpdatedAt })
                .ToListAsync();
            return Results.Ok(stories);
        });
    }
}
"""

admin_get_story = """using InteractiveStory.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class GetStoryEndpoint
{
    public static void MapGetStory(this RouteGroupBuilder group)
    {
        group.MapGet("/{id}", async (string id, AppDbContext db) =>
        {
            var story = await db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            return story != null ? Results.Ok(story) : Results.NotFound();
        });
    }
}
"""

admin_get_playthroughs = """using System.Linq;
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
        group.MapGet("/{id}/playthroughs", async (string id, AppDbContext db) =>
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
"""

admin_create_story = """using System;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Models;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class CreateStoryEndpoint
{
    public static void MapCreateStory(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateStoryRequest req, AppDbContext db) =>
        {
            var id = SlugGenerator.Generate(req.Title);
            
            // basic check for collision, highly unlikely with slug gen but good to have
            if (await db.Stories.AnyAsync(s => s.Id == id))
                id = SlugGenerator.Generate(req.Title);

            var story = new Story
            {
                Id = id,
                Title = req.Title,
                Description = req.Description,
                Json = req.Json,
                Password = req.Password,
                MaxPlaythroughs = req.MaxPlaythroughs,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Stories.Add(story);
            await db.SaveChangesAsync();

            return Results.Created($"/api/admin/stories/{story.Id}", story);
        });
    }
}

public class CreateStoryRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Json { get; set; } = null!;
    public string? Password { get; set; }
    public int? MaxPlaythroughs { get; set; }
}
"""

admin_update_story = """using System;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class UpdateStoryEndpoint
{
    public static void MapUpdateStory(this RouteGroupBuilder group)
    {
        group.MapPut("/{id}", async (string id, UpdateStoryRequest req, AppDbContext db, StoryCacheService cache) =>
        {
            var story = await db.Stories.FirstOrDefaultAsync(s => s.Id == id);
            if (story == null) return Results.NotFound();

            story.Title = req.Title;
            story.Description = req.Description;
            story.Json = req.Json;
            story.Password = req.Password;
            story.MaxPlaythroughs = req.MaxPlaythroughs;
            story.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            cache.InvalidateCache(id);

            return Results.Ok(story);
        });
    }
}

public class UpdateStoryRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Json { get; set; } = null!;
    public string? Password { get; set; }
    public int? MaxPlaythroughs { get; set; }
}
"""

admin_delete_story = """using InteractiveStory.Api.Data;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class DeleteStoryEndpoint
{
    public static void MapDeleteStory(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id}", async (string id, AppDbContext db, StoryCacheService cache) =>
        {
            var story = await db.Stories.FirstOrDefaultAsync(s => s.Id == id);
            if (story == null) return Results.NoContent();

            var hasPlaythroughs = await db.Playthroughs.AnyAsync(p => p.StoryId == id);
            if (hasPlaythroughs)
            {
                return Results.BadRequest("Cannot delete story because it has playthroughs.");
            }

            db.Stories.Remove(story);
            await db.SaveChangesAsync();
            cache.InvalidateCache(id);

            return Results.NoContent();
        });
    }
}
"""

admin_upload_media = """using System;
using System.IO;
using System.Linq;
using InteractiveStory.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class UploadStoryMediaEndpoint
{
    public static void MapUploadStoryMedia(this RouteGroupBuilder group)
    {
        group.MapPost("/{id}/media", async (string id, IFormFile file, AppDbContext db) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var validExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".txt" };
            if (!validExts.Contains(ext))
                return Results.BadRequest("Invalid file type.");

            // create folder
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", id);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var filePath = Path.Combine(dir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Results.Ok(new { Url = $"/media/{id}/{fileName}" });
        }).DisableAntiforgery();
    }
}
"""

admin_validate_story = """using System.Linq;
using System.Text.Json;
using InteractiveStory.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class ValidateStoryEndpoint
{
    public static void MapValidateStory(this RouteGroupBuilder group)
    {
        group.MapPost("/{id}/validate", (string id, ValidateStoryRequest req) =>
        {
            try
            {
                var def = JsonSerializer.Deserialize<StoryDefinition>(req.Json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (def == null) return Results.BadRequest("Invalid JSON");
                if (!def.Nodes.ContainsKey(def.StartNode))
                    return Results.BadRequest($"Start node '{def.StartNode}' not found in nodes dictionary.");

                var errors = new System.Collections.Generic.List<string>();
                foreach (var (nodeId, node) in def.Nodes)
                {
                    if (!node.IsEnding && (node.Choices == null || node.Choices.Count == 0))
                    {
                        errors.Add($"Node '{nodeId}' is not an ending node but has no choices.");
                    }

                    if (node.Choices != null)
                    {
                        foreach (var choice in node.Choices)
                        {
                            if (!def.Nodes.ContainsKey(choice.Next))
                            {
                                errors.Add($"Choice '{choice.Id}' in node '{nodeId}' points to missing node '{choice.Next}'.");
                            }
                        }
                    }
                }

                if (errors.Any()) return Results.BadRequest(new { Errors = errors });

                return Results.Ok(new { Valid = true });
            }
            catch (JsonException ex)
            {
                return Results.BadRequest($"Invalid JSON format: {ex.Message}");
            }
        });
    }
}

public class ValidateStoryRequest
{
    public string Json { get; set; } = null!;
}
"""

admin_endpoints = """using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/stories");

        group.MapGetStories();
        group.MapGetStory();
        group.MapGetStoryPlaythroughs();
        group.MapCreateStory();
        group.MapUpdateStory();
        group.MapDeleteStory();
        group.MapUploadStoryMedia();
        group.MapValidateStory();
    }
}
"""

player_start_playthrough = """using System;
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
    }
}

public class StartPlaythroughRequest
{
    public string? Password { get; set; }
}
"""

player_get_current_node = """using System;
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
    }
}
"""

player_make_choice = """using System;
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
    }
}

public class ChoiceRequest
{
    public string ChoiceId { get; set; } = null!;
}
"""

player_get_history = """using System;
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
"""

player_endpoints = """using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace InteractiveStory.Api.Endpoints.Player;

public static class PlayerEndpoints
{
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        group.MapStartPlaythrough();
        group.MapGetCurrentNode();
        group.MapMakeChoice();
        group.MapGetPlaythroughHistory();
    }
}
"""

with open(os.path.join(admin_dir, "GetStoriesEndpoint.cs"), "w") as f: f.write(admin_get_stories)
with open(os.path.join(admin_dir, "GetStoryEndpoint.cs"), "w") as f: f.write(admin_get_story)
with open(os.path.join(admin_dir, "GetStoryPlaythroughsEndpoint.cs"), "w") as f: f.write(admin_get_playthroughs)
with open(os.path.join(admin_dir, "CreateStoryEndpoint.cs"), "w") as f: f.write(admin_create_story)
with open(os.path.join(admin_dir, "UpdateStoryEndpoint.cs"), "w") as f: f.write(admin_update_story)
with open(os.path.join(admin_dir, "DeleteStoryEndpoint.cs"), "w") as f: f.write(admin_delete_story)
with open(os.path.join(admin_dir, "UploadStoryMediaEndpoint.cs"), "w") as f: f.write(admin_upload_media)
with open(os.path.join(admin_dir, "ValidateStoryEndpoint.cs"), "w") as f: f.write(admin_validate_story)
with open(os.path.join(admin_dir, "AdminEndpoints.cs"), "w") as f: f.write(admin_endpoints)

with open(os.path.join(player_dir, "StartPlaythroughEndpoint.cs"), "w") as f: f.write(player_start_playthrough)
with open(os.path.join(player_dir, "GetCurrentNodeEndpoint.cs"), "w") as f: f.write(player_get_current_node)
with open(os.path.join(player_dir, "MakeChoiceEndpoint.cs"), "w") as f: f.write(player_make_choice)
with open(os.path.join(player_dir, "GetPlaythroughHistoryEndpoint.cs"), "w") as f: f.write(player_get_history)
with open(os.path.join(player_dir, "PlayerEndpoints.cs"), "w") as f: f.write(player_endpoints)

if os.path.exists(os.path.join(base_dir, "AdminEndpoints.cs")):
    os.remove(os.path.join(base_dir, "AdminEndpoints.cs"))
if os.path.exists(os.path.join(base_dir, "PlayerEndpoints.cs")):
    os.remove(os.path.join(base_dir, "PlayerEndpoints.cs"))

