using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Models;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace InteractiveStory.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/stories");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var stories = await db.Stories
                .AsNoTracking()
                .Select(s => new { s.Id, s.Title, s.Description, s.UpdatedAt })
                .ToListAsync();
            return Results.Ok(stories);
        });

        group.MapGet("/{id}", async (string id, AppDbContext db) =>
        {
            var story = await db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            return story != null ? Results.Ok(story) : Results.NotFound();
        });

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

public class CreateStoryRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Json { get; set; } = null!;
    public string? Password { get; set; }
    public int? MaxPlaythroughs { get; set; }
}

public class UpdateStoryRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Json { get; set; } = null!;
    public string? Password { get; set; }
    public int? MaxPlaythroughs { get; set; }
}

public class ValidateStoryRequest
{
    public string Json { get; set; } = null!;
}
