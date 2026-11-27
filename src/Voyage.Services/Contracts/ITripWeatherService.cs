using Voyage.Web.ViewModels.Weather;

namespace Voyage.Services.Contracts
{
    public interface ITripWeatherService
    {
        Task<TripWeatherViewModel?> GetForTripAsync(int tripId, string userId);
    }
}
