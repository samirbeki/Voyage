using Microsoft.AspNetCore.Identity;
using Voyage.Data;
using Voyage.Data.Models;

namespace Voyage.Services.Tests.Infrastructure
{
    public static class TestData
    {
        public static async Task<IdentityUser> AddUserAsync(ApplicationDbContext dbContext, string email)
        {
            var user = new IdentityUser { UserName = email, Email = email };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }

        public static async Task<Category> AddCategoryAsync(ApplicationDbContext dbContext, string name)
        {
            var category = new Category { Name = name };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return category;
        }

        public static async Task<Destination> AddDestinationAsync(
            ApplicationDbContext dbContext,
            int categoryId,
            string name,
            string country = "Bulgaria",
            bool isDeleted = false)
        {
            var destination = new Destination
            {
                Name = name,
                Country = country,
                Description = $"{name} is a place worth visiting.",
                CategoryId = categoryId,
                IsDeleted = isDeleted
            };

            dbContext.Destinations.Add(destination);
            await dbContext.SaveChangesAsync();

            return destination;
        }

        public static async Task<Review> AddReviewAsync(
            ApplicationDbContext dbContext,
            string authorId,
            int destinationId,
            int rating,
            bool isHidden = false,
            bool isDeleted = false)
        {
            var review = new Review
            {
                AuthorId = authorId,
                DestinationId = destinationId,
                Rating = rating,
                Comment = "A short review used in tests.",
                CreatedOn = DateTime.UtcNow,
                IsHidden = isHidden,
                IsDeleted = isDeleted
            };

            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();

            return review;
        }

        public static async Task<Trip> AddTripAsync(
            ApplicationDbContext dbContext,
            string ownerId,
            string title = "Test trip",
            int startInDays = 10,
            int lengthInDays = 5,
            decimal budget = 1000m,
            params int[] destinationIds)
        {
            var startDate = DateTime.Today.AddDays(startInDays);

            var trip = new Trip
            {
                Title = title,
                StartDate = startDate,
                EndDate = startDate.AddDays(lengthInDays),
                Budget = budget,
                CreatedOn = DateTime.UtcNow,
                OwnerId = ownerId
            };

            foreach (var destinationId in destinationIds)
            {
                trip.TripDestinations.Add(new TripDestination { DestinationId = destinationId });
            }

            dbContext.Trips.Add(trip);
            await dbContext.SaveChangesAsync();

            return trip;
        }
    }
}
