using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models;

// Matches the "Roles" entity in the Part 1 ERD.
public class Role
{
    [Key]
    public int RoleID { get; set; }

    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
