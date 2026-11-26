using Voyage.Web.ViewModels.Weather;

namespace Voyage.Services.Contracts
{
    public interface IWeatherService
    {
        Task<WeatherViewModel?> GetForecastAsync(string place, string country);
    }
}
