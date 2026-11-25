using Voyage.Web.ViewModels.TravelExperts;

namespace Voyage.Services.Contracts
{
    public interface ITravelExpertService
    {
        Task<TravelExpertsPageViewModel> GetUsersAsync(string? search);

        Task<string?> GrantAsync(string userId);

        Task<string?> RemoveAsync(string userId);
    }
}
