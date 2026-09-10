using System;
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
            var id = Guid.NewGuid();

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
