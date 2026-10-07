using System.ComponentModel.DataAnnotations;

namespace RaceDay.Dtos;

public class ResultDto
{
    public int ResultID { get; set; }
    public int EnrollmentID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int? Position { get; set; }
    public DateTime CapturedDate { get; set; }
}

public class CreateResultDto
{
    [Required]
    public TimeSpan FinishTime { get; set; }

    public int? Position { get; set; }
}
