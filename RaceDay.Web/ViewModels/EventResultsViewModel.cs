namespace RaceDay.Web.ViewModels
{
    public class EventResultsViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;

        public List<EnrolmentViewModel> Enrolments { get; set; } = new();
        public List<ResultViewModel> Results { get; set; } = new();
    }
}