using Voyage.Web.Common;
using Voyage.Web.ViewModels.Packing;

namespace Voyage.Web.Services.Contracts
{
    public interface IPackingService
    {
        Task<PackingListViewModel?> GetListAsync(int tripId, string userId);

        Task<PackingProgressViewModel> GetProgressAsync(int tripId);

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
    }
}