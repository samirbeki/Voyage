using Voyage.Data.Models.Enums;
using Voyage.Web.ViewModels.Expenses;

namespace Voyage.Services.Helpers
{
    internal static class ExpenseCategorySummaries
    {
        public static List<ExpenseCategorySummaryViewModel> Create(
            IEnumerable<(ExpenseCategory Category, decimal Amount)> expenses)
        {
            var list = expenses.ToList();
            var total = list.Sum(e => e.Amount);

            if (total <= 0)
            {
                return new List<ExpenseCategorySummaryViewModel>();
            }

            return list
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseCategorySummaryViewModel
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count(),
                    Share = Math.Round(g.Sum(e => e.Amount) / total * 100, 1)
                })
                .OrderByDescending(c => c.Amount)
                .ThenBy(c => c.Category)
                .ToList();
        }
    }
}
