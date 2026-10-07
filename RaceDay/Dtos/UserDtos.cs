using System.ComponentModel.DataAnnotations;

namespace RaceDay.Dtos;

public class UserProfileDto
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class UpdateProfileDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}
