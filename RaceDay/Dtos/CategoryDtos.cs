using System.ComponentModel.DataAnnotations;

namespace RaceDay.Dtos;

public class CategoryDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public int MaxParticipants { get; set; }
}

public class CreateCategoryDto
{
    [Required]
    public string CategoryName { get; set; } = string.Empty;

    [Range(0, 10000)]
    public decimal DistanceKm { get; set; }

    [Range(0, 100000)]
    public decimal EntryFee { get; set; }

    [Range(1, 1000000)]
    public int MaxParticipants { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto
{
}
