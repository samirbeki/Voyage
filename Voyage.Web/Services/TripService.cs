using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data;
using Voyage.Web.Data.Entities;
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

        public async Task<IEnumerable<DestinationOptionViewModel>> GetDestinationOptionsAsync()
        {
            return await dbContext.Destinations
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new DestinationOptionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Country = d.Country
                })
                .ToListAsync();
        }

        public async Task<bool> DestinationsExistAsync(IEnumerable<int> destinationIds)
        {
            var ids = destinationIds.Distinct().ToList();

            var count = await dbContext.Destinations
                .CountAsync(d => ids.Contains(d.Id) && !d.IsDeleted);

            return count == ids.Count;
        }

        public async Task<int> CreateAsync(TripFormViewModel model, string userId)
        {
            var trip = new Trip
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim(),
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Budget = model.Budget,
                CoverImageUrl = model.CoverImageUrl,
                OwnerId = userId,
                CreatedOn = DateTime.UtcNow
            };

            foreach (var id in model.SelectedDestinationIds.Distinct())
            {
                trip.TripDestinations.Add(new TripDestination
                {
                    DestinationId = id
                });
            }

            await dbContext.Trips.AddAsync(trip);

            await dbContext.SaveChangesAsync();

            return trip.Id;
        }
    }
}