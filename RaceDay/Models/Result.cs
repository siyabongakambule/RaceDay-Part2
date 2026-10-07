using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models;

// Matches the "Results" entity in the Part 1 ERD.
// One enrolment can have at most one result (enforced by a unique constraint on EnrollmentID).
public class Result
{
    [Key]
    public int ResultID { get; set; }

    [Required]
    public int EnrollmentID { get; set; }

    [ForeignKey(nameof(EnrollmentID))]
    public Enrollment? Enrollment { get; set; }

    [Required]
    public int CapturedByUserID { get; set; }

    [ForeignKey(nameof(CapturedByUserID))]
    public User? CapturedByUser { get; set; }

    [Required]
    public TimeSpan FinishTime { get; set; }

    public int? Position { get; set; }

    public DateTime CapturedDate { get; set; } = DateTime.UtcNow;
}
