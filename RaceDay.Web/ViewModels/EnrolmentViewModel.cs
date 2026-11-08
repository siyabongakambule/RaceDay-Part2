namespace RaceDay.Web.ViewModels
{
    public class EnrolmentViewModel
    {
        public int EnrollmentID { get; set; }

        public int ParticipantID { get; set; }

        public string? ParticipantName { get; set; }

        public int CategoryID { get; set; }

        public string? CategoryName { get; set; }

        public int EventID { get; set; }

        public string? EventName { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string? Status { get; set; }
    }
}
