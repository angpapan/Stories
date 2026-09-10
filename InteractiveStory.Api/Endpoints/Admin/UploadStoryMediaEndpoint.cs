using System;
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
        group.MapPost("/{id}/media", async (Guid id, IFormFile file, AppDbContext db, IWebHostEnvironment env) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var validExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".txt" };
            if (!validExts.Contains(ext))
                return Results.BadRequest("Invalid file type.");

            // create folder
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", id.ToString());
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
