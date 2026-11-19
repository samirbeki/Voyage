using Voyage.Web.ViewModels.Reviews;

namespace Voyage.Web.Services.Contracts
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewViewModel>> GetForDestinationAsync(
            int destinationId, string? userId, bool includeHidden);

        Task<RatingSummaryViewModel> GetRatingSummaryAsync(int destinationId);

        Task<bool> HasUserReviewedAsync(int destinationId, string userId);

        Task<bool> CreateAsync(int destinationId, ReviewFormModel model, string userId);

        Task<ReviewEditViewModel?> GetForEditAsync(int id, string userId);

        Task<int?> EditAsync(int id, ReviewFormModel model, string userId);

        Task<int?> DeleteAsync(int id, string userId);
    }
}