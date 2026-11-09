using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Please enter an event name.")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an event type.")]
        public string EventType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an event date.")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Please enter a province.")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a venue.")]
        public string Venue { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
