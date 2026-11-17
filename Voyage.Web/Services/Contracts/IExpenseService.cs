using Voyage.Web.ViewModels.Expenses;

namespace Voyage.Web.Services.Contracts
{
    public interface IExpenseService
    {
        Task<ExpenseFormViewModel?> GetAddFormAsync(int tripId, string userId);

        Task<bool> IsDateWithinTripAsync(int tripId, DateTime date);

        Task<bool> AddAsync(ExpenseFormViewModel model, string userId);

        Task<ExpenseFormViewModel?> GetForEditAsync(int id, string userId);

        Task<bool> EditAsync(int id, ExpenseFormViewModel model, string userId);

        Task<ExpenseViewModel?> GetForDeleteAsync(int id, string userId);

        Task<int?> DeleteAsync(int id, string userId);
    }
}