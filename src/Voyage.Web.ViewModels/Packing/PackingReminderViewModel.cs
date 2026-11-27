namespace Voyage.Web.ViewModels.Packing
{
    public class PackingReminderViewModel
    {
        public int TripId { get; set; }

        public string TripTitle { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public int DaysUntilStart { get; set; }

        public int Total { get; set; }

        public int Packed { get; set; }

        public int ItemsLeft => Total - Packed;

        public bool IsListEmpty => Total == 0;
    }
}
