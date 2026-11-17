using Voyage.Web.Common;

namespace Voyage.Web.ViewModels.Expenses
{
    public class ExpenseViewModel
    {
        public int Id { get; set; }

        public int TripId { get; set; }

        public string Title { get; set; } = null!;

        public decimal Amount { get; set; }

        public ExpenseCategory Category { get; set; }

        public DateTime Date { get; set; }
    }
}