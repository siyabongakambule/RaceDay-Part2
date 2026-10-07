using RaceDay.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models;

// Matches the "EventCategories" entity in the Part 1 ERD.
// Used for age or distance categories, e.g. "Under 20", "10km-20km".
public class EventCategory
{
    [Key]
    public int CategoryID { get; set; }

    [Required]
    public int EventID { get; set; }

    [ForeignKey(nameof(EventID))]
    public Event? Event { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(6,2)")]
    public decimal DistanceKm { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal EntryFee { get; set; }

    public int MaxParticipants { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
