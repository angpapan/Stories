using System;

namespace InteractiveStory.Api.Models;

public class Story
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Json { get; set; } = null!;
    public string? Password { get; set; }
    public string? PasswordHint { get; set; }
    public int? MaxPlaythroughs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
