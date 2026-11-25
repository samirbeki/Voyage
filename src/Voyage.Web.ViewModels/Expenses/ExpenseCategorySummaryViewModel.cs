using Voyage.Data.Models.Enums;

namespace Voyage.Web.ViewModels.Expenses
{
    public class ExpenseCategorySummaryViewModel
    {
        public ExpenseCategory Category { get; set; }

        public decimal Amount { get; set; }

        public int Count { get; set; }

        public decimal Share { get; set; }
    }
}
