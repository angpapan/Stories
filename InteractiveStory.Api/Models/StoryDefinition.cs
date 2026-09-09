using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InteractiveStory.Api.Models;

public class StoryDefinition
{
    [JsonPropertyName("startNode")]
    public string StartNode { get; set; } = null!;

    [JsonPropertyName("nodes")]
    public Dictionary<string, NodeDefinition> Nodes { get; set; } = new();
}

public class NodeDefinition
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;

    [JsonPropertyName("isEnding")]
    public bool IsEnding { get; set; }

    [JsonPropertyName("media")]
    public List<MediaDefinition> Media { get; set; } = new();

    [JsonPropertyName("choices")]
    public List<ChoiceDefinition> Choices { get; set; } = new();
}

public class ChoiceDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("text")]
    public string Text { get; set; } = null!;

    [JsonPropertyName("next")]
    public string Next { get; set; } = null!;

    [JsonPropertyName("media")]
    public List<MediaDefinition> Media { get; set; } = new();
}

public class MediaDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("caption")]
    public string Caption { get; set; } = "";
}
