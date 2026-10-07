using System.ComponentModel.DataAnnotations;

namespace RaceDay.Dtos;

public class EnrolmentDto
{
    public int EnrollmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateEnrolmentStatusDto
{
    // Expected values: "Pending", "Confirmed", "Cancelled"
    [Required]
    public string Status { get; set; } = string.Empty;
}
