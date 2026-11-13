using Microsoft.EntityFrameworkCore;
using Voyage.Web.Common;
using Voyage.Web.Data;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Destinations;
using Voyage.Web.ViewModels.Home;

namespace Voyage.Web.Services
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
                .Select(d => new DestinationAllViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Country = d.Country,
                    ImageUrl = d.ImageUrl,
                    CategoryName = d.Category.Name,
                    Description = d.Description
                })
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