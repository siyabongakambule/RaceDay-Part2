using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models;

// Matches the "Enrollments" entity in the Part 1 ERD.
// Links a participant (User) to the category (and therefore event) they entered.
public class Enrollment
{
    [Key]
    public int EnrollmentID { get; set; }

    [Required]
    public int ParticipantID { get; set; }

    [ForeignKey(nameof(ParticipantID))]
    public User? Participant { get; set; }

    [Required]
    public int CategoryID { get; set; }

    [ForeignKey(nameof(CategoryID))]
    public EventCategory? Category { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    // Expected values: "Pending", "Confirmed", "Cancelled"
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public Result? Result { get; set; }
}
