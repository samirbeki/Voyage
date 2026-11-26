using Microsoft.EntityFrameworkCore;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Contracts;
using Voyage.Services.Helpers;
using Voyage.Web.ViewModels.Enums;
using Voyage.Web.ViewModels.Trips;

namespace Voyage.Services
{
    public class TripSharingService : ITripSharingService
    {
        private readonly ApplicationDbContext dbContext;

        public TripSharingService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Guid?> ShareAsync(int tripId, string userId)
        {
            var trip = await FindOwnedTripAsync(tripId, userId);

            if (trip == null)
            {
                return null;
            }

            if (trip.ShareToken == null)
            {
                trip.ShareToken = Guid.NewGuid();
                await dbContext.SaveChangesAsync();
            }

            return trip.ShareToken;
        }

        public async Task<Guid?> RegenerateAsync(int tripId, string userId)
        {
            var trip = await FindOwnedTripAsync(tripId, userId);

            if (trip == null)
            {
                return null;
            }

            trip.ShareToken = Guid.NewGuid();
            await dbContext.SaveChangesAsync();

            return trip.ShareToken;
        }

        public async Task<bool> StopSharingAsync(int tripId, string userId)
        {
            var trip = await FindOwnedTripAsync(tripId, userId);

            if (trip == null)
            {
                return false;
            }

            if (trip.ShareToken != null)
            {
                trip.ShareToken = null;
                await dbContext.SaveChangesAsync();
            }

            return true;
        }

        public async Task<SharedTripViewModel?> GetSharedAsync(Guid token)
        {
            if (token == Guid.Empty)
            {
                return null;
            }

            var trip = await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.ShareToken == token)
                .Select(t => new
                {
                    t.Title,
                    t.Description,
                    t.StartDate,
                    t.EndDate,
                    OwnerName = t.Owner.UserName!,
                    CoverImageUrl = t.CoverImageUrl == null || t.CoverImageUrl == ""
                        ? t.TripDestinations
                            .Where(td => !td.Destination.IsDeleted)
                            .OrderBy(td => td.Destination.Name)
                            .Select(td => td.Destination.ImageUrl)
                            .FirstOrDefault()
                        : t.CoverImageUrl,
                    Destinations = t.TripDestinations
                        .Where(td => !td.Destination.IsDeleted)
                        .OrderBy(td => td.Destination.Name)
                        .Select(td => new TripDestinationViewModel
                        {
                            Id = td.Destination.Id,
                            Name = td.Destination.Name,
                            Country = td.Destination.Country,
                            ImageUrl = td.Destination.ImageUrl
                        })
                        .ToList(),
                    Expenses = t.Expenses
                        .Select(e => new { e.Category, e.Amount })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return null;
            }

            var today = DateTime.Today;
            int atIndex = trip.OwnerName.IndexOf('@');

            return new SharedTripViewModel
            {
                Title = trip.Title,
                Description = trip.Description,
                OwnerName = atIndex > 0 ? trip.OwnerName[..atIndex] : trip.OwnerName,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                DaysCount = (trip.EndDate - trip.StartDate).Days + 1,
                Status = trip.EndDate < today
                    ? TripStatus.Completed
                    : trip.StartDate > today
                        ? TripStatus.Upcoming
                        : TripStatus.Ongoing,
                CoverImageUrl = trip.CoverImageUrl,
                Destinations = trip.Destinations,
                TotalSpent = trip.Expenses.Sum(e => e.Amount),
                ExpensesByCategory = ExpenseCategorySummaries.Create(
                    trip.Expenses.Select(e => (e.Category, e.Amount)))
            };
        }

        private async Task<Trip?> FindOwnedTripAsync(int tripId, string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            return await dbContext.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.OwnerId == userId);
        }
    }
}
