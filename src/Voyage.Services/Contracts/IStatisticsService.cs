using Voyage.Web.ViewModels.Statistics;

namespace Voyage.Services.Contracts
{
    public interface IStatisticsService
    {
        Task<UserStatisticsViewModel> GetForUserAsync(string userId);
    }
}
