using System;

namespace InteractiveStory.Api.Models;

public enum PlaythroughStatus
{
    InProgress,
    Completed
}

public class Playthrough
{
    public Guid Id { get; set; }
    public Guid StoryId { get; set; }
    public string CurrentNodeId { get; set; } = null!;
    public PlaythroughStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public Story Story { get; set; } = null!;
}
