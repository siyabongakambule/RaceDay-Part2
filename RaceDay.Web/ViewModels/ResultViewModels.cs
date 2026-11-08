namespace RaceDay.Web.ViewModels
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int EnrollmentID { get; set; }

        public string? ParticipantName { get; set; }

        public string? EventName { get; set; }

        public string? CategoryName { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int? Position { get; set; }

        public DateTime CapturedDate { get; set; }
    }
}
