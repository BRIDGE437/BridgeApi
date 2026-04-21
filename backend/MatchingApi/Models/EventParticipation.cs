using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchingApi.Models;

public class EventParticipation
{
    [Key]
    public int Id { get; set; }

    public int EventId { get; set; }
    
    [ForeignKey(nameof(EventId))]
    public MatchEvent? Event { get; set; }

    [Required, MaxLength(100)]
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>"Investor" or "Startup"</summary>
    [Required, MaxLength(20)]
    public string ParticipantType { get; set; } = string.Empty; 

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
