using System;

namespace InteractiveStory.Api.Models;

public class Story
{
    public string Id { get; set; } = null!; // GUID or slug
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Json { get; set; } = null!;
    public string? Password { get; set; }
    public int? MaxPlaythroughs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
