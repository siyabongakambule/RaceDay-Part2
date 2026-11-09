using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Please enter a category name.")]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 1000, ErrorMessage = "Enter a valid distance.")]
        public decimal DistanceKm { get; set; }

        [Required]
        [Range(0, 100000, ErrorMessage = "Enter a valid entry fee.")]
        public decimal EntryFee { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
        public int MaxParticipants { get; set; }
    }
}
