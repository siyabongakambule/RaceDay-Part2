namespace RaceDay.Web.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string? OrganiserName { get; set; }
        public string? EventName { get; set; }
        public string? EventType { get; set; }
        public DateTime EventDate { get; set; }
        public string? Province { get; set; }
        public string? Venue { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
