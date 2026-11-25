using Microsoft.EntityFrameworkCore;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Results;
using Voyage.Services.Tests.Infrastructure;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class PackingServicePackedStateTests : IDisposable
    {
        private readonly SqliteTestDatabase database;
        private readonly ApplicationDbContext dbContext;
        private readonly PackingService service;

        public PackingServicePackedStateTests()
        {
            database = new SqliteTestDatabase();
            dbContext = database.DbContext;
            service = new PackingService(dbContext);
        }

        public void Dispose()
        {
            database.Dispose();
        }

        [Fact]
        public async Task TogglePackedAsync_UnpackedItem_MarksItAsPacked()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Istanbul Food Week");
            var item = await AddItemAsync(trip.Id, "Comfortable shoes");

            var result = await service.TogglePackedAsync(item.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.True(result.IsPacked);
            Assert.Equal(trip.Id, result.TripId);
            Assert.True(await IsPackedAsync(item.Id));
        }

        [Fact]
        public async Task TogglePackedAsync_PackedItem_MarksItAsUnpacked()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Comfortable shoes", isPacked: true);

            var result = await service.TogglePackedAsync(item.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.False(result.IsPacked);
            Assert.False(await IsPackedAsync(item.Id));
        }

        [Fact]
        public async Task TogglePackedAsync_TwiceInARow_ReturnsToTheStartState()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Scarf");

            await service.TogglePackedAsync(item.Id, user.Id);
            var result = await service.TogglePackedAsync(item.Id, user.Id);

            Assert.False(result.IsPacked);
            Assert.False(await IsPackedAsync(item.Id));
        }

        [Fact]
        public async Task TogglePackedAsync_ChangesOnlyTheSelectedItem()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Scarf");
            var other = await AddItemAsync(trip.Id, "Gloves");

            await service.TogglePackedAsync(item.Id, user.Id);

            Assert.False(await IsPackedAsync(other.Id));
        }

        [Fact]
        public async Task TogglePackedAsync_ItemOfAnotherUser_ReturnsItemNotFoundAndKeepsState()
        {
            var owner = await AddUserAsync(dbContext, "ana@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var item = await AddItemAsync(trip.Id, "Scarf");

            var result = await service.TogglePackedAsync(item.Id, stranger.Id);

            Assert.False(result.Succeeded);
            Assert.Equal(PackingResult.ItemNotFound, result.Result);
            Assert.False(await IsPackedAsync(item.Id));
        }

        [Fact]
        public async Task TogglePackedAsync_MissingItem_ReturnsItemNotFound()
        {
            var user = await AddUserAsync(dbContext, "ana@voyage.test");

            var result = await service.TogglePackedAsync(999, user.Id);

            Assert.Equal(PackingResult.ItemNotFound, result.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task TogglePackedAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.TogglePackedAsync(1, userId!));
        }

        [Fact]
        public async Task UnpackAllAsync_PackedItems_UnpacksAllOfThem()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Weekend in Thessaloniki");
            await AddItemAsync(trip.Id, "Passport", isPacked: true);
            await AddItemAsync(trip.Id, "Swimsuit", isPacked: true);
            await AddItemAsync(trip.Id, "Sun cream", isPacked: true);
            await AddItemAsync(trip.Id, "Book");

            var result = await service.UnpackAllAsync(trip.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(3, result.Affected);
            Assert.False(await dbContext.PackingItems.AsNoTracking().AnyAsync(p => p.IsPacked));
            Assert.Equal(4, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task UnpackAllAsync_NothingPacked_ReturnsZeroAffected()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "Book");

            var result = await service.UnpackAllAsync(trip.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(0, result.Affected);
        }

        [Fact]
        public async Task UnpackAllAsync_TripWithoutItems_ReturnsZeroAffected()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            var result = await service.UnpackAllAsync(trip.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(0, result.Affected);
        }

        [Fact]
        public async Task UnpackAllAsync_DoesNotChangeItemsOfOtherTrips()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Thessaloniki");
            var otherTrip = await AddTripAsync(dbContext, user.Id, "Halkidiki");
            await AddItemAsync(trip.Id, "Passport", isPacked: true);
            var otherItem = await AddItemAsync(otherTrip.Id, "Snorkel", isPacked: true);

            var result = await service.UnpackAllAsync(trip.Id, user.Id);

            Assert.Equal(1, result.Affected);
            Assert.True(await IsPackedAsync(otherItem.Id));
        }

        [Fact]
        public async Task UnpackAllAsync_TripOfAnotherUser_ReturnsTripNotFoundAndKeepsItemsPacked()
        {
            var owner = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var item = await AddItemAsync(trip.Id, "Passport", isPacked: true);

            var result = await service.UnpackAllAsync(trip.Id, stranger.Id);

            Assert.False(result.Succeeded);
            Assert.Equal(PackingResult.TripNotFound, result.Result);
            Assert.Equal(0, result.Affected);
            Assert.True(await IsPackedAsync(item.Id));
        }

        [Fact]
        public async Task UnpackAllAsync_MissingTrip_ReturnsTripNotFound()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");

            var result = await service.UnpackAllAsync(999, user.Id);

            Assert.Equal(PackingResult.TripNotFound, result.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task UnpackAllAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.UnpackAllAsync(1, userId!));
        }

        private async Task<PackingItem> AddItemAsync(int tripId, string name, bool isPacked = false)
        {
            var item = new PackingItem
            {
                TripId = tripId,
                Name = name,
                Category = PackingCategory.Other,
                Quantity = 1,
                IsPacked = isPacked,
                CreatedOn = DateTime.UtcNow
            };

            dbContext.PackingItems.Add(item);
            await dbContext.SaveChangesAsync();

            return item;
        }

        private async Task<bool> IsPackedAsync(int itemId)
        {
            return await dbContext.PackingItems
                .AsNoTracking()
                .Where(p => p.Id == itemId)
                .Select(p => p.IsPacked)
                .SingleAsync();
        }
    }
}
