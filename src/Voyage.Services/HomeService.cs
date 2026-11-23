using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Services.Contracts;
using Voyage.Services.Projections;
using Voyage.Web.ViewModels.Home;

namespace Voyage.Services
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext dbContext;

        public HomeService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<HomeIndexViewModel> GetIndexAsync()
        {
            var destinationsCount = await dbContext.Destinations.CountAsync();

            var categoriesCount = await dbContext.Categories.CountAsync();

            var tripsCount = await dbContext.Trips.CountAsync();

            var popularDestinations = await dbContext.Destinations
                .AsNoTracking()
                .OrderByDescending(d => d.TripDestinations.Count)
                .ThenBy(d => d.Name)
                .Take(PagingConstants.PopularDestinationsCount)
                .Select(DestinationProjections.ToCard)
                .ToListAsync();

            return new HomeIndexViewModel
            {
                PopularDestinations = popularDestinations,
                DestinationsCount = destinationsCount,
                CategoriesCount = categoriesCount,
                TripsCount = tripsCount
            };
        }
    }
}
