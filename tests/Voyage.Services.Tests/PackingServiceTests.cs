using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants.Validation;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Results;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Packing;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class PackingServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly PackingService service;

        public PackingServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new PackingService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetListAsync_OwnTrip_ReturnsTripInfoAndLimit()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Vienna Christmas Markets", 30, 4);

            var result = await service.GetListAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(trip.Id, result.TripId);
            Assert.Equal("Vienna Christmas Markets", result.TripName);
            Assert.Equal(trip.StartDate, result.StartDate);
            Assert.Equal(PackingItemValidationConstants.MaxPerTrip, result.MaxItems);
        }

        [Fact]
        public async Task GetListAsync_TripWithoutItems_ReturnsEmptyGroupsAndZeroProgress()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            var result = await service.GetListAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Empty(result.Groups);
            Assert.Equal(0, result.Progress.Total);
            Assert.Equal(0, result.Progress.Percent);
            Assert.False(result.Progress.IsComplete);
        }

        [Fact]
        public async Task GetListAsync_ItemsInDifferentCategories_GroupsThemInCategoryOrder()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "Travel insurance", PackingCategory.Documents);
            await AddItemAsync(trip.Id, "Warm jacket", PackingCategory.Clothing);
            await AddItemAsync(trip.Id, "Painkillers", PackingCategory.Health);
            await AddItemAsync(trip.Id, "Passport", PackingCategory.Documents);

            var result = await service.GetListAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(
                new[] { PackingCategory.Clothing, PackingCategory.Documents, PackingCategory.Health },
                result.Groups.Select(g => g.Category));
            Assert.Equal(2, result.Groups.Single(g => g.Category == PackingCategory.Documents).TotalCount);
        }

        [Fact]
        public async Task GetListAsync_ItemsInGroup_UnpackedFirstThenByName()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "T-shirts", PackingCategory.Clothing, isPacked: true);
            await AddItemAsync(trip.Id, "Socks", PackingCategory.Clothing);
            await AddItemAsync(trip.Id, "Jeans", PackingCategory.Clothing);
            await AddItemAsync(trip.Id, "Hat", PackingCategory.Clothing, isPacked: true);

            var result = await service.GetListAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            var group = Assert.Single(result.Groups);
            Assert.Equal(new[] { "Jeans", "Socks", "Hat", "T-shirts" }, group.Items.Select(i => i.Name));
        }

        [Fact]
        public async Task GetListAsync_PackedItems_CountsProgressPerGroupAndInTotal()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "Toothbrush", PackingCategory.Toiletries, isPacked: true);
            await AddItemAsync(trip.Id, "Shampoo", PackingCategory.Toiletries);
            await AddItemAsync(trip.Id, "Phone charger", PackingCategory.Electronics, isPacked: true);
            await AddItemAsync(trip.Id, "Camera", PackingCategory.Electronics, isPacked: true);

            var result = await service.GetListAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            var toiletries = result.Groups.Single(g => g.Category == PackingCategory.Toiletries);
            var electronics = result.Groups.Single(g => g.Category == PackingCategory.Electronics);
            Assert.Equal(1, toiletries.PackedCount);
            Assert.Equal(2, toiletries.TotalCount);
            Assert.Equal(2, electronics.PackedCount);
            Assert.Equal(4, result.Progress.Total);
            Assert.Equal(3, result.Progress.Packed);
            Assert.Equal(75, result.Progress.Percent);
        }

        [Fact]
        public async Task GetListAsync_ReturnsItemDetails()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Socks (pairs)", PackingCategory.Clothing, quantity: 5, isPacked: true);

            var result = await service.GetListAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            var model = Assert.Single(Assert.Single(result.Groups).Items);
            Assert.Equal(item.Id, model.Id);
            Assert.Equal("Socks (pairs)", model.Name);
            Assert.Equal(5, model.Quantity);
            Assert.Equal(PackingCategory.Clothing, model.Category);
            Assert.True(model.IsPacked);
        }

        [Fact]
        public async Task GetListAsync_DoesNotIncludeItemsFromOtherTrips()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Vienna");
            var otherTrip = await AddTripAsync(dbContext, user.Id, "Budapest");
            await AddItemAsync(trip.Id, "Gloves", PackingCategory.Clothing);
            await AddItemAsync(otherTrip.Id, "Swimsuit", PackingCategory.Clothing);

            var result = await service.GetListAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            var item = Assert.Single(Assert.Single(result.Groups).Items);
            Assert.Equal("Gloves", item.Name);
            Assert.Equal(1, result.Progress.Total);
        }

        [Fact]
        public async Task GetListAsync_TripOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "elena@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            await AddItemAsync(trip.Id, "Passport", PackingCategory.Documents);

            Assert.Null(await service.GetListAsync(trip.Id, stranger.Id));
        }

        [Fact]
        public async Task GetListAsync_MissingTrip_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");

            Assert.Null(await service.GetListAsync(999, user.Id));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetListAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.GetListAsync(1, userId!));
        }

        [Fact]
        public async Task GetProgressAsync_TripWithoutItems_ReturnsZero()
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            var result = await service.GetProgressAsync(trip.Id);

            Assert.Equal(0, result.Total);
            Assert.Equal(0, result.Packed);
            Assert.Equal(0, result.Percent);
            Assert.False(result.IsComplete);
        }

        [Theory]
        [InlineData(1, 3, 33)]
        [InlineData(2, 3, 67)]
        [InlineData(1, 2, 50)]
        [InlineData(0, 4, 0)]
        public async Task GetProgressAsync_PartlyPacked_ReturnsRoundedPercent(int packed, int total, int expectedPercent)
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            for (int i = 0; i < total; i++)
            {
                await AddItemAsync(trip.Id, $"Item {i + 1}", PackingCategory.Other, isPacked: i < packed);
            }

            var result = await service.GetProgressAsync(trip.Id);

            Assert.Equal(total, result.Total);
            Assert.Equal(packed, result.Packed);
            Assert.Equal(expectedPercent, result.Percent);
            Assert.False(result.IsComplete);
        }

        [Fact]
        public async Task GetProgressAsync_EverythingPacked_IsComplete()
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "Passport", PackingCategory.Documents, isPacked: true);
            await AddItemAsync(trip.Id, "Power bank", PackingCategory.Electronics, isPacked: true);

            var result = await service.GetProgressAsync(trip.Id);

            Assert.Equal(100, result.Percent);
            Assert.True(result.IsComplete);
        }

        [Fact]
        public async Task GetProgressAsync_CountsOnlyItemsOfTheGivenTrip()
        {
            var user = await AddUserAsync(dbContext, "ivan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Plovdiv");
            var otherTrip = await AddTripAsync(dbContext, user.Id, "Veliko Tarnovo");
            await AddItemAsync(trip.Id, "Sunglasses", PackingCategory.Other, isPacked: true);
            await AddItemAsync(otherTrip.Id, "Umbrella", PackingCategory.Other);
            await AddItemAsync(otherTrip.Id, "Raincoat", PackingCategory.Clothing);

            var result = await service.GetProgressAsync(trip.Id);

            Assert.Equal(1, result.Total);
            Assert.True(result.IsComplete);
        }

        [Fact]
        public async Task AddAsync_OwnTrip_SavesUnpackedItem()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Rila Lakes Hike");
            var before = DateTime.UtcNow;

            var result = await service.AddAsync(trip.Id, CreateModel("Hiking boots", 1, PackingCategory.Clothing), user.Id);

            Assert.Equal(PackingResult.Success, result);
            var item = await dbContext.PackingItems.SingleAsync();
            Assert.Equal("Hiking boots", item.Name);
            Assert.Equal(1, item.Quantity);
            Assert.Equal(PackingCategory.Clothing, item.Category);
            Assert.Equal(trip.Id, item.TripId);
            Assert.False(item.IsPacked);
            Assert.InRange(item.CreatedOn, before, DateTime.UtcNow);
        }

        [Fact]
        public async Task AddAsync_NameWithExtraSpaces_SavesNormalizedName()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            var result = await service.AddAsync(trip.Id, CreateModel("  Sun    cream  SPF 50 ", 2, PackingCategory.Toiletries), user.Id);

            Assert.Equal(PackingResult.Success, result);
            var item = await dbContext.PackingItems.SingleAsync();
            Assert.Equal("Sun cream SPF 50", item.Name);
            Assert.Equal(2, item.Quantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        [InlineData(999)]
        public async Task AddAsync_InvalidOrMissingTrip_ReturnsTripNotFound(int tripId)
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");

            var result = await service.AddAsync(tripId, CreateModel("Map"), user.Id);

            Assert.Equal(PackingResult.TripNotFound, result);
            Assert.Empty(dbContext.PackingItems);
        }

        [Fact]
        public async Task AddAsync_TripOfAnotherUser_ReturnsTripNotFound()
        {
            var owner = await AddUserAsync(dbContext, "georgi@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            var result = await service.AddAsync(trip.Id, CreateModel("Map"), stranger.Id);

            Assert.Equal(PackingResult.TripNotFound, result);
            Assert.Empty(dbContext.PackingItems);
        }

        [Fact]
        public async Task AddAsync_UndefinedCategory_ReturnsInvalidCategory()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);

            var result = await service.AddAsync(trip.Id, CreateModel("Map", 1, (PackingCategory)42), user.Id);

            Assert.Equal(PackingResult.InvalidCategory, result);
            Assert.Empty(dbContext.PackingItems);
        }

        [Fact]
        public async Task AddAsync_ItemAlreadyInTheList_ReturnsDuplicate()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "Power bank", PackingCategory.Electronics);

            var result = await service.AddAsync(trip.Id, CreateModel("Power bank", 1, PackingCategory.Electronics), user.Id);

            Assert.Equal(PackingResult.Duplicate, result);
            Assert.Equal(1, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddAsync_SameNameWithExtraSpaces_ReturnsDuplicate()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "Travel adapter", PackingCategory.Electronics);

            var result = await service.AddAsync(trip.Id, CreateModel("  Travel    adapter "), user.Id);

            Assert.Equal(PackingResult.Duplicate, result);
            Assert.Equal(1, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddAsync_SameNameInAnotherTrip_IsAllowed()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Bansko");
            var otherTrip = await AddTripAsync(dbContext, user.Id, "Borovets");
            await AddItemAsync(otherTrip.Id, "Ski goggles", PackingCategory.Other);

            var result = await service.AddAsync(trip.Id, CreateModel("Ski goggles"), user.Id);

            Assert.Equal(PackingResult.Success, result);
            Assert.Equal(2, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddAsync_LimitReached_ReturnsLimitReached()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await FillTripAsync(trip.Id, PackingItemValidationConstants.MaxPerTrip);

            var result = await service.AddAsync(trip.Id, CreateModel("One more thing"), user.Id);

            Assert.Equal(PackingResult.LimitReached, result);
            Assert.Equal(PackingItemValidationConstants.MaxPerTrip, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddAsync_OneBelowTheLimit_AddsTheLastItem()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await FillTripAsync(trip.Id, PackingItemValidationConstants.MaxPerTrip - 1);

            var result = await service.AddAsync(trip.Id, CreateModel("Last thing"), user.Id);

            Assert.Equal(PackingResult.Success, result);
            Assert.Equal(PackingItemValidationConstants.MaxPerTrip, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task AddAsync_ItemsOfOtherTrips_DoNotCountTowardsTheLimit()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sofia");
            var fullTrip = await AddTripAsync(dbContext, user.Id, "Lisbon");
            await FillTripAsync(fullTrip.Id, PackingItemValidationConstants.MaxPerTrip);

            var result = await service.AddAsync(trip.Id, CreateModel("Metro card"), user.Id);

            Assert.Equal(PackingResult.Success, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.AddAsync(1, CreateModel("Map"), userId!));
        }

        [Fact]
        public async Task GetForEditAsync_OwnItem_ReturnsItemAndTripInfo()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Amsterdam by Bike");
            var item = await AddItemAsync(trip.Id, "Rain jacket", PackingCategory.Clothing, quantity: 2);

            var result = await service.GetForEditAsync(item.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(item.Id, result.Id);
            Assert.Equal(trip.Id, result.TripId);
            Assert.Equal("Amsterdam by Bike", result.TripName);
            Assert.Equal("Rain jacket", result.Name);
            Assert.Equal(2, result.Quantity);
            Assert.Equal(PackingCategory.Clothing, result.Category);
        }

        [Fact]
        public async Task GetForEditAsync_ItemOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "petya@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var item = await AddItemAsync(trip.Id, "Rain jacket", PackingCategory.Clothing);

            Assert.Null(await service.GetForEditAsync(item.Id, stranger.Id));
        }

        [Fact]
        public async Task GetForEditAsync_MissingItem_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");

            Assert.Null(await service.GetForEditAsync(999, user.Id));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetForEditAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.GetForEditAsync(1, userId!));
        }

        [Fact]
        public async Task EditAsync_OwnItem_UpdatesNameQuantityAndCategory()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Charger", PackingCategory.Other);

            var result = await service.EditAsync(item.Id, CreateModel("  Laptop   charger ", 1, PackingCategory.Electronics), user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(trip.Id, result.TripId);
            var saved = await dbContext.PackingItems.AsNoTracking().SingleAsync();
            Assert.Equal("Laptop charger", saved.Name);
            Assert.Equal(PackingCategory.Electronics, saved.Category);
        }

        [Fact]
        public async Task EditAsync_OnlyQuantityChanged_KeepsTheSameNameWithoutDuplicateError()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "T-shirts", PackingCategory.Clothing, quantity: 3);

            var result = await service.EditAsync(item.Id, CreateModel("T-shirts", 5, PackingCategory.Clothing), user.Id);

            Assert.True(result.Succeeded);
            var saved = await dbContext.PackingItems.AsNoTracking().SingleAsync();
            Assert.Equal(5, saved.Quantity);
        }

        [Fact]
        public async Task EditAsync_PackedItem_StaysPacked()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Passport", PackingCategory.Documents, isPacked: true);

            var result = await service.EditAsync(item.Id, CreateModel("Passport and ID card", 1, PackingCategory.Documents), user.Id);

            Assert.True(result.Succeeded);
            Assert.True(result.IsPacked);
            Assert.True((await dbContext.PackingItems.AsNoTracking().SingleAsync()).IsPacked);
        }

        [Fact]
        public async Task EditAsync_NameOfAnotherItemInTheSameTrip_ReturnsDuplicateAndKeepsOldName()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            await AddItemAsync(trip.Id, "Toothpaste", PackingCategory.Toiletries);
            var item = await AddItemAsync(trip.Id, "Toothbrush", PackingCategory.Toiletries);

            var result = await service.EditAsync(item.Id, CreateModel("Toothpaste", 1, PackingCategory.Toiletries), user.Id);

            Assert.False(result.Succeeded);
            Assert.Equal(PackingResult.Duplicate, result.Result);
            var saved = await dbContext.PackingItems.AsNoTracking().SingleAsync(p => p.Id == item.Id);
            Assert.Equal("Toothbrush", saved.Name);
        }

        [Fact]
        public async Task EditAsync_NameOfItemInAnotherTrip_IsAllowed()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Bansko");
            var otherTrip = await AddTripAsync(dbContext, user.Id, "Pamporovo");
            await AddItemAsync(otherTrip.Id, "Ski gloves", PackingCategory.Clothing);
            var item = await AddItemAsync(trip.Id, "Gloves", PackingCategory.Clothing);

            var result = await service.EditAsync(item.Id, CreateModel("Ski gloves", 1, PackingCategory.Clothing), user.Id);

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task EditAsync_UndefinedCategory_ReturnsInvalidCategory()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Book", PackingCategory.Other);

            var result = await service.EditAsync(item.Id, CreateModel("Book", 1, (PackingCategory)(-1)), user.Id);

            Assert.Equal(PackingResult.InvalidCategory, result.Result);
            Assert.Equal(PackingCategory.Other, (await dbContext.PackingItems.AsNoTracking().SingleAsync()).Category);
        }

        [Fact]
        public async Task EditAsync_ItemOfAnotherUser_ReturnsItemNotFound()
        {
            var owner = await AddUserAsync(dbContext, "petya@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var item = await AddItemAsync(trip.Id, "Book", PackingCategory.Other);

            var result = await service.EditAsync(item.Id, CreateModel("Stolen book"), stranger.Id);

            Assert.Equal(PackingResult.ItemNotFound, result.Result);
            Assert.Equal(0, result.TripId);
            Assert.Equal("Book", (await dbContext.PackingItems.AsNoTracking().SingleAsync()).Name);
        }

        [Fact]
        public async Task EditAsync_MissingItem_ReturnsItemNotFound()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");

            var result = await service.EditAsync(999, CreateModel("Book"), user.Id);

            Assert.Equal(PackingResult.ItemNotFound, result.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task EditAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.EditAsync(1, CreateModel("Book"), userId!));
        }

        [Fact]
        public async Task DeleteAsync_OwnItem_RemovesItAndReturnsTripId()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Snorkel", PackingCategory.Other);

            var result = await service.DeleteAsync(item.Id, user.Id);

            Assert.True(result.Succeeded);
            Assert.Equal(trip.Id, result.TripId);
            Assert.Empty(dbContext.PackingItems);
        }

        [Fact]
        public async Task DeleteAsync_RemovesOnlyTheSelectedItem()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Snorkel", PackingCategory.Other);
            await AddItemAsync(trip.Id, "Flip-flops", PackingCategory.Clothing);
            await AddItemAsync(trip.Id, "Beach towel", PackingCategory.Other);

            await service.DeleteAsync(item.Id, user.Id);

            var names = await dbContext.PackingItems.Select(p => p.Name).OrderBy(n => n).ToListAsync();
            Assert.Equal(new[] { "Beach towel", "Flip-flops" }, names);
        }

        [Fact]
        public async Task DeleteAsync_ItemOfAnotherUser_ReturnsItemNotFoundAndKeepsIt()
        {
            var owner = await AddUserAsync(dbContext, "stefan@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var item = await AddItemAsync(trip.Id, "Snorkel", PackingCategory.Other);

            var result = await service.DeleteAsync(item.Id, stranger.Id);

            Assert.False(result.Succeeded);
            Assert.Equal(PackingResult.ItemNotFound, result.Result);
            Assert.Single(dbContext.PackingItems);
        }

        [Fact]
        public async Task DeleteAsync_MissingItem_ReturnsItemNotFound()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");

            var result = await service.DeleteAsync(999, user.Id);

            Assert.Equal(PackingResult.ItemNotFound, result.Result);
        }

        [Fact]
        public async Task DeleteAsync_SameItemTwice_SecondTimeReturnsItemNotFound()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var item = await AddItemAsync(trip.Id, "Snorkel", PackingCategory.Other);

            await service.DeleteAsync(item.Id, user.Id);
            var result = await service.DeleteAsync(item.Id, user.Id);

            Assert.Equal(PackingResult.ItemNotFound, result.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task DeleteAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.DeleteAsync(1, userId!));
        }

        private async Task<PackingItem> AddItemAsync(
            int tripId,
            string name,
            PackingCategory category,
            int quantity = 1,
            bool isPacked = false)
        {
            var item = new PackingItem
            {
                TripId = tripId,
                Name = name,
                Category = category,
                Quantity = quantity,
                IsPacked = isPacked,
                CreatedOn = DateTime.UtcNow
            };

            dbContext.PackingItems.Add(item);
            await dbContext.SaveChangesAsync();

            return item;
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

        private static PackingItemFormModel CreateModel(
            string name,
            int quantity = 1,
            PackingCategory category = PackingCategory.Other)
        {
            return new PackingItemFormModel
            {
                Name = name,
                Quantity = quantity,
                Category = category
            };
        }
    }
}
