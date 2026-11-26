using Voyage.Web.ViewModels.Trips;

namespace Voyage.Services.Contracts
{
    public interface ITripSharingService
    {
        Task<Guid?> ShareAsync(int tripId, string userId);

        Task<Guid?> RegenerateAsync(int tripId, string userId);

        Task<bool> StopSharingAsync(int tripId, string userId);

        Task<SharedTripViewModel?> GetSharedAsync(Guid token);
    }
}
