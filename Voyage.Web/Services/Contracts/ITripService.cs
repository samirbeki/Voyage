using Voyage.Web.ViewModels.Trips;

namespace Voyage.Web.Services.Contracts
{
    public interface ITripService
    {
        Task<IEnumerable<TripAllViewModel>> GetMineAsync(string userId);
    }
}