using System.ComponentModel.DataAnnotations;

namespace RaceDay.Dtos;

public class EventDto
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Province { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateEventDto
{
    [Required]
    public string EventName { get; set; } = string.Empty;

    // "Run" or "Cycle"
    [Required]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public string Province { get; set; } = string.Empty;

    [Required]
    public string Venue { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateEventDto : CreateEventDto
{
}
