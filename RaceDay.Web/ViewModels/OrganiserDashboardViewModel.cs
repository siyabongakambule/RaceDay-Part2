namespace RaceDay.Web.ViewModels
{
    public class OrganiserDashboardViewModel
    {
        public int TotalEvents { get; set; }

        public int TotalEnrolments { get; set; }

        public int UpcomingEvents { get; set; }

        public List<EventViewModel> Events { get; set; } = new();
    }
}