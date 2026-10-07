using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models;

// Matches the "Events" entity in the Part 1 ERD.
// EventType captures whether the event is a run or a cycle event.
public class Event
{
    [Key]
    public int EventID { get; set; }

    [Required]
    public int OrganiserID { get; set; }

    [ForeignKey(nameof(OrganiserID))]
    public User? Organiser { get; set; }

    [Required]
    [MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    // Expected values: "Run" or "Cycle"
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Venue { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<EventCategory> Categories { get; set; } = new List<EventCategory>();
}
