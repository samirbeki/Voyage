using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.Weather;

namespace Voyage.Services
{
    public class TripWeatherService : ITripWeatherService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IWeatherService weatherService;

        public TripWeatherService(ApplicationDbContext dbContext, IWeatherService weatherService)
        {
            this.dbContext = dbContext;
            this.weatherService = weatherService;
        }

        public async Task<TripWeatherViewModel?> GetForTripAsync(int tripId, string userId)
        {
            var trip = await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.Id == tripId && t.OwnerId == userId)
                .Select(t => new
                {
                    t.StartDate,
                    t.EndDate,
                    Destinations = t.TripDestinations
                        .Where(td => !td.Destination.IsDeleted)
                        .OrderBy(td => td.Destination.Name)
                        .Select(td => new
                        {
                            td.Destination.Id,
                            td.Destination.Name,
                            td.Destination.Country
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return null;
            }

            var today = DateTime.Today;
            var lastForecastDay = today.AddDays(WeatherConstants.MaxForecastDays - 1);
            var startDate = trip.StartDate.Date;
            var endDate = trip.EndDate.Date;

            var model = new TripWeatherViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                AvailableFrom = startDate.AddDays(-(WeatherConstants.MaxForecastDays - 1)),
                CoveredUntil = endDate < lastForecastDay ? endDate : lastForecastDay,
                IsFinished = endDate < today,
                IsTooEarly = startDate > lastForecastDay
            };

            if (model.IsFinished || model.IsTooEarly)
            {
                return model;
            }

            var firstDay = startDate > today ? startDate : today;
            var destinations = new List<TripDestinationWeatherViewModel>();

            foreach (var destination in trip.Destinations)
            {
                var forecast = await weatherService.GetForecastAsync(destination.Name, destination.Country);
                var weather = forecast?.ForPeriod(firstDay, model.CoveredUntil);

                destinations.Add(new TripDestinationWeatherViewModel
                {
                    DestinationId = destination.Id,
                    Name = destination.Name,
                    Country = destination.Country,
                    Weather = weather != null && weather.Days.Any() ? weather : null
                });
            }

            model.Destinations = destinations;

            return model;
        }
    }
}
