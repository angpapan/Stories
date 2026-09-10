using System.Linq;
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
        group.MapPost("/{id}/validate", (Guid id, ValidateStoryRequest req) =>
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
