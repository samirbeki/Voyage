using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Trips;

namespace Voyage.Web.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext dbContext;

        public TripService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<TripAllViewModel>> GetMineAsync(string userId)
        {
            return await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.OwnerId == userId)
                .OrderBy(t => t.StartDate)
                .Select(t => new TripAllViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Budget = t.Budget,
                    CoverImageUrl = t.CoverImageUrl,
                    DestinationsCount = t.TripDestinations.Count
                })
                .ToListAsync();
        }
    }
}