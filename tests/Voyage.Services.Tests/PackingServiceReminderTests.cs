using Voyage.Common.Constants.Validation;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Tests.Infrastructure;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class PackingServiceReminderTests : IDisposable
    {
        private readonly SqliteTestDatabase database;
        private readonly ApplicationDbContext dbContext;
        private readonly PackingService service;

        public PackingServiceReminderTests()
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
        public async Task GetRemindersAsync_TripSoonWithUnpackedItems_ReturnsReminder()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Weekend in Plovdiv", startInDays: 3);
            await AddItemAsync(trip.Id, "Passport", isPacked: true);
            await AddItemAsync(trip.Id, "Camera");
            await AddItemAsync(trip.Id, "Sunglasses");

            var reminders = (await service.GetRemindersAsync(user.Id)).ToList();

            var reminder = Assert.Single(reminders);
            Assert.Equal(trip.Id, reminder.TripId);
            Assert.Equal("Weekend in Plovdiv", reminder.TripTitle);
            Assert.Equal(trip.StartDate, reminder.StartDate);
            Assert.Equal(3, reminder.DaysUntilStart);
            Assert.Equal(3, reminder.Total);
            Assert.Equal(1, reminder.Packed);
            Assert.Equal(2, reminder.ItemsLeft);
            Assert.False(reminder.IsListEmpty);
        }

        [Fact]
        public async Task GetRemindersAsync_EmptyPackingList_ReturnsReminderForEmptyList()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            await AddTripAsync(dbContext, user.Id, startInDays: 2);

            var reminder = Assert.Single(await service.GetRemindersAsync(user.Id));

            Assert.True(reminder.IsListEmpty);
            Assert.Equal(0, reminder.Total);
            Assert.Equal(0, reminder.ItemsLeft);
        }

        [Fact]
        public async Task GetRemindersAsync_EverythingPacked_ReturnsNoReminders()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, startInDays: 2);
            await AddItemAsync(trip.Id, "Passport", isPacked: true);
            await AddItemAsync(trip.Id, "Camera", isPacked: true);

            var reminders = await service.GetRemindersAsync(user.Id);

            Assert.Empty(reminders);
        }

        [Fact]
        public async Task GetRemindersAsync_TripStartingToday_ReturnsZeroDaysUntilStart()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            await AddTripAsync(dbContext, user.Id, startInDays: 0);

            var reminder = Assert.Single(await service.GetRemindersAsync(user.Id));

            Assert.Equal(0, reminder.DaysUntilStart);
        }

        [Fact]
        public async Task GetRemindersAsync_TripOnLastDayOfTheWindow_ReturnsReminder()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            await AddTripAsync(
                dbContext,
                user.Id,
                startInDays: PackingItemValidationConstants.ReminderDaysBeforeStart);

            var reminder = Assert.Single(await service.GetRemindersAsync(user.Id));

            Assert.Equal(PackingItemValidationConstants.ReminderDaysBeforeStart, reminder.DaysUntilStart);
        }

        [Fact]
        public async Task GetRemindersAsync_TripAfterTheWindow_ReturnsNoReminders()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            await AddTripAsync(
                dbContext,
                user.Id,
                startInDays: PackingItemValidationConstants.ReminderDaysBeforeStart + 1);

            var reminders = await service.GetRemindersAsync(user.Id);

            Assert.Empty(reminders);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task GetRemindersAsync_TripAlreadyStarted_ReturnsNoReminders(int startInDays)
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            await AddTripAsync(dbContext, user.Id, startInDays: startInDays);

            var reminders = await service.GetRemindersAsync(user.Id);

            Assert.Empty(reminders);
        }

        [Fact]
        public async Task GetRemindersAsync_TripOfAnotherUser_IsNotReturned()
        {
            var owner = await AddUserAsync(dbContext, "petya@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            await AddTripAsync(dbContext, owner.Id, startInDays: 2);

            var reminders = await service.GetRemindersAsync(stranger.Id);

            Assert.Empty(reminders);
        }

        [Fact]
        public async Task GetRemindersAsync_SeveralTrips_OrdersByStartDateAndLimitsTheCount()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            await AddTripAsync(dbContext, user.Id, "Fourth", startInDays: 6);
            await AddTripAsync(dbContext, user.Id, "Second", startInDays: 2);
            await AddTripAsync(dbContext, user.Id, "First", startInDays: 1);
            await AddTripAsync(dbContext, user.Id, "Third", startInDays: 4);

            var titles = (await service.GetRemindersAsync(user.Id))
                .Select(r => r.TripTitle)
                .ToList();

            Assert.Equal(PackingItemValidationConstants.MaxReminders, titles.Count);
            Assert.Equal(new[] { "First", "Second", "Third" }, titles);
        }

        [Fact]
        public async Task GetRemindersAsync_PackedTripIsSkipped_NextTripTakesItsPlace()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var packedTrip = await AddTripAsync(dbContext, user.Id, "Packed", startInDays: 1);
            await AddItemAsync(packedTrip.Id, "Passport", isPacked: true);
            await AddTripAsync(dbContext, user.Id, "Not packed", startInDays: 5);

            var reminder = Assert.Single(await service.GetRemindersAsync(user.Id));

            Assert.Equal("Not packed", reminder.TripTitle);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetRemindersAsync_EmptyUserId_Throws(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.GetRemindersAsync(userId!));
        }

        private async Task AddItemAsync(int tripId, string name, bool isPacked = false)
        {
            dbContext.PackingItems.Add(new PackingItem
            {
                TripId = tripId,
                Name = name,
                Category = PackingCategory.Other,
                Quantity = 1,
                IsPacked = isPacked,
                CreatedOn = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
    }
}
