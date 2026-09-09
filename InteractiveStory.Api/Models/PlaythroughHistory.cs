using System;

namespace InteractiveStory.Api.Models;

public class PlaythroughHistory
{
    public int Id { get; set; }
    public Guid PlaythroughId { get; set; }
    public string NodeId { get; set; } = null!;
    public string? ChosenChoiceId { get; set; }
    public int SequenceNumber { get; set; }
    public DateTime AnsweredAt { get; set; }

    public Playthrough Playthrough { get; set; } = null!;
}
