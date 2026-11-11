using Voyage.Web.ViewModels.Trips;

namespace Voyage.Web.Services.Contracts
{
    public interface ITripService
    {
        Task<IEnumerable<TripAllViewModel>> GetMineAsync(string userId);

        Task<IEnumerable<DestinationOptionViewModel>> GetDestinationOptionsAsync();

        Task<bool> DestinationsExistAsync(IEnumerable<int> destinationIds);

        Task<int> CreateAsync(TripFormViewModel model, string userId);

        Task<TripDetailsViewModel?> GetDetailsAsync(int id, string userId);

        Task<TripFormViewModel?> GetForEditAsync(int id, string userId);

        Task<bool> EditAsync(int id, TripFormViewModel model, string userId);

        Task<bool> DeleteAsync(int id, string userId);
    }
}