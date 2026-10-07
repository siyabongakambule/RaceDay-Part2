using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace RaceDay.Models;

// Matches the "Users" entity in the Part 1 ERD.
public class User
{
    [Key]
    public int UserID { get; set; }

    [Required]
    public int RoleID { get; set; }

    [ForeignKey(nameof(RoleID))]
    public Role? Role { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    // Stores the PBKDF2 password hash - never the plain text password.
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Result> CapturedResults { get; set; } = new List<Result>();
}
