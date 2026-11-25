namespace Voyage.Web.ViewModels.Expenses
{
    public class ExpenseChartViewModel
    {
        public IEnumerable<ExpenseCategorySummaryViewModel> Categories { get; set; }
            = new List<ExpenseCategorySummaryViewModel>();

        public decimal Total { get; set; }
    }
}
