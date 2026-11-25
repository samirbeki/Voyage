using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants.Validation;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Results;
using Voyage.Services.Templates;
using Voyage.Services.Tests.Infrastructure;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class PackingServiceEssentialsTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly PackingService service;

        public PackingServiceEssentialsTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new PackingService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task AddEssentialsAsync_EmptyList_AddsAllTemplateItemsUnpacked()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Krakow and Zakopane", 20, 4);
            var expected = PackingTemplates.GetEssentials(5);

            var result = await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(expected.Count, result.Affected);
            Assert.Equal(0, result.NotAddedDueToLimit);

            var items = await dbContext.PackingItems.ToListAsync();
            Assert.Equal(expected.Count, items.Count);
            Assert.All(items, i => Assert.False(i.IsPacked));
            Assert.All(items, i => Assert.Equal(trip.Id, i.TripId));
            Assert.Equal(
                expected.Select(t => t.Name).OrderBy(n => n),
                items.Select(i => i.Name).OrderBy(n => n));
        }

        [Fact]
        public async Task AddEssentialsAsync_SavesCategoriesFromTheTemplate()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            await service.AddEssentialsAsync(trip.Id, user.Id);

            var items = await dbContext.PackingItems.ToListAsync();
            Assert.Equal(PackingCategory.Documents, items.Single(i => i.Name == "Passport / ID card").Category);
            Assert.Equal(PackingCategory.Electronics, items.Single(i => i.Name == "Power bank").Category);
            Assert.Equal(PackingCategory.Health, items.Single(i => i.Name == "First aid kit").Category);
        }

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(4, 5, 3)]
        [InlineData(6, 7, 4)]
        [InlineData(13, 14, 7)]
        public async Task AddEssentialsAsync_ClothingQuantity_DependsOnTripLength(int lengthInDays, int expectedDaily, int expectedShirts)
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, lengthInDays: lengthInDays);

            await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.Equal(expectedDaily, await QuantityOfAsync("Underwear"));
            Assert.Equal(expectedDaily, await QuantityOfAsync("Socks (pairs)"));
            Assert.Equal(expectedShirts, await QuantityOfAsync("T-shirts"));
            Assert.Equal(1, await QuantityOfAsync("Toothbrush"));
        }

        [Fact]
        public async Task AddEssentialsAsync_VeryLongTrip_QuantityStaysWithinTheLimit()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, lengthInDays: 150);

            await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.Equal(PackingItemValidationConstants.QuantityMax, await QuantityOfAsync("Underwear"));
            Assert.Equal(50, await QuantityOfAsync("T-shirts"));
        }

        [Fact]
        public async Task AddEssentialsAsync_SomeItemsAlreadyInTheList_AddsOnlyTheMissingOnes()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var templateCount = PackingTemplates.GetEssentials(6).Count;
            await AddItemAsync(trip.Id, "Toothbrush", PackingCategory.Toiletries, isPacked: true);
            await AddItemAsync(trip.Id, "Power bank", PackingCategory.Electronics);

            var result = await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(templateCount - 2, result.Affected);
            Assert.Equal(templateCount, await dbContext.PackingItems.CountAsync());
            Assert.Single(dbContext.PackingItems.Where(p => p.Name == "Toothbrush"));
            Assert.True((await dbContext.PackingItems.SingleAsync(p => p.Name == "Toothbrush")).IsPacked);
        }

        [Fact]
        public async Task AddEssentialsAsync_ExistingItemWithDifferentCase_IsNotAddedAgain()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var templateCount = PackingTemplates.GetEssentials(6).Count;
            await AddItemAsync(trip.Id, "PASSPORT / id card", PackingCategory.Documents);

            var result = await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.Equal(templateCount - 1, result.Affected);
            Assert.DoesNotContain(dbContext.PackingItems, p => p.Name == "Passport / ID card");
        }

        [Fact]
        public async Task AddEssentialsAsync_CalledTwice_SecondTimeAddsNothing()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            var first = await service.AddEssentialsAsync(trip.Id, user.Id);
            var second = await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.True(second.Succeeded);
            Assert.Equal(0, second.Affected);
            Assert.Equal(first.Affected, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddEssentialsAsync_AlmostFullList_AddsUntilTheLimitAndReportsTheRest()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var templateCount = PackingTemplates.GetEssentials(6).Count;
            await FillTripAsync(trip.Id, PackingItemValidationConstants.MaxPerTrip - 5);

            var result = await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(5, result.Affected);
            Assert.Equal(templateCount - 5, result.NotAddedDueToLimit);
            Assert.Equal(PackingItemValidationConstants.MaxPerTrip, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddEssentialsAsync_AlmostFullList_AddsItemsInTemplateOrder()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await FillTripAsync(trip.Id, PackingItemValidationConstants.MaxPerTrip - 2);

            await service.AddEssentialsAsync(trip.Id, user.Id);

            var added = await dbContext.PackingItems
                .Where(p => !p.Name.StartsWith("Item "))
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToListAsync();

            Assert.Equal(new[] { "Passport / ID card", "Tickets & bookings" }, added);
        }

        [Fact]
        public async Task AddEssentialsAsync_FullList_ReturnsLimitReached()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await FillTripAsync(trip.Id, PackingItemValidationConstants.MaxPerTrip);

            var result = await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.False(result.Succeeded);
            Assert.Equal(PackingResult.LimitReached, result.Result);
            Assert.Equal(PackingItemValidationConstants.MaxPerTrip, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddEssentialsAsync_ItemsOfOtherTrips_AreNotTakenIntoAccount()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Krakow");
            var otherTrip = await AddTripAsync(dbContext, user.Id, "Gdansk");
            var templateCount = PackingTemplates.GetEssentials(6).Count;
            await AddItemAsync(otherTrip.Id, "Toothbrush", PackingCategory.Toiletries);

            var result = await service.AddEssentialsAsync(trip.Id, user.Id);

            Assert.Equal(templateCount, result.Affected);
        }

        [Fact]
        public async Task AddEssentialsAsync_TripOfAnotherUser_ReturnsTripNotFound()
        {
            var owner = await AddUserAsync(dbContext, "nikolay@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            var result = await service.AddEssentialsAsync(trip.Id, stranger.Id);

            Assert.Equal(PackingResult.TripNotFound, result.Result);
            Assert.Empty(dbContext.PackingItems);
        }

        [Fact]
        public async Task AddEssentialsAsync_MissingTrip_ReturnsTripNotFound()
        {
            var user = await AddUserAsync(dbContext, "nikolay@voyage.test");

            var result = await service.AddEssentialsAsync(999, user.Id);

            Assert.Equal(PackingResult.TripNotFound, result.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddEssentialsAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.AddEssentialsAsync(1, userId!));
        }

        private async Task<int> QuantityOfAsync(string name)
        {
            return await dbContext.PackingItems
                .Where(p => p.Name == name)
                .Select(p => p.Quantity)
                .SingleAsync();
        }

        private async Task AddItemAsync(int tripId, string name, PackingCategory category, bool isPacked = false)
        {
            dbContext.PackingItems.Add(new PackingItem
            {
                TripId = tripId,
                Name = name,
                Category = category,
                Quantity = 1,
                IsPacked = isPacked,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }

        private async Task FillTripAsync(int tripId, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                dbContext.PackingItems.Add(new PackingItem
                {
                    TripId = tripId,
                    Name = $"Item {i}",
                    Category = PackingCategory.Other,
                    Quantity = 1,
                    CreatedOn = DateTime.UtcNow
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
