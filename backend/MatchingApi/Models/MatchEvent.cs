using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchingApi.Models;

public class MatchEvent
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    /// <summary>Upcoming, Open, Processing, Completed</summary>
    [MaxLength(50)]
    public string Status { get; set; } = "Upcoming"; 

    public int TopMatchingCount { get; set; } = 5;

    [MaxLength(50)]
    public string? EventType { get; set; }

    [MaxLength(100)]
    public string? FilterValue { get; set; }

    public ICollection<EventParticipation> Participations { get; set; } = new List<EventParticipation>();
    public ICollection<MatchResult> MatchResults { get; set; } = new List<MatchResult>();
}
