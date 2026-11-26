using Voyage.Services.Results;
using Voyage.Web.ViewModels.ExpertTips;

namespace Voyage.Services.Contracts
{
    public interface IExpertTipService
    {
        Task<IEnumerable<ExpertTipViewModel>> GetForDestinationAsync(int destinationId, string? userId);

        Task<bool> CanAddAsync(int destinationId, string userId);

        Task<ExpertTipResult> CreateAsync(int destinationId, ExpertTipFormModel model, string userId);

        Task<ExpertTipEditViewModel?> GetForEditAsync(int id, string userId);

        Task<int?> EditAsync(int id, ExpertTipFormModel model, string userId);

        Task<int?> DeleteAsync(int id, string userId);

        Task<int?> DeleteByModeratorAsync(int id);
    }
}
