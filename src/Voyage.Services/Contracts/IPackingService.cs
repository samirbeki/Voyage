using Voyage.Services.Results;
using Voyage.Web.ViewModels.Packing;

namespace Voyage.Services.Contracts
{
    public interface IPackingService
    {
        Task<PackingListViewModel?> GetListAsync(int tripId, string userId);

        Task<PackingProgressViewModel> GetProgressAsync(int tripId);

        Task<IEnumerable<PackingReminderViewModel>> GetRemindersAsync(string userId);

        Task<PackingResult> AddAsync(
            int tripId,
            PackingItemFormModel model,
            string userId);

        Task<PackingItemEditViewModel?> GetForEditAsync(int id, string userId);

        Task<PackingOperationResult> EditAsync(
            int id,
            PackingItemFormModel model,
            string userId);

        Task<PackingOperationResult> DeleteAsync(int id, string userId);

        Task<PackingOperationResult> TogglePackedAsync(int id, string userId);

        Task<PackingBulkResult> AddEssentialsAsync(int tripId, string userId);

        Task<PackingBulkResult> UnpackAllAsync(int tripId, string userId);
    }
}
