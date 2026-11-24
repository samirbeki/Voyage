using Microsoft.EntityFrameworkCore;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Expenses;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class ExpenseServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ExpenseService service;

        public ExpenseServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new ExpenseService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetAddFormAsync_OwnTrip_ReturnsFormWithTripInfoAndStartDate()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Greek Island Hopping", 60, 7);

            var result = await service.GetAddFormAsync(trip.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(trip.Id, result.TripId);
            Assert.Equal("Greek Island Hopping", result.TripTitle);
            Assert.Equal(trip.StartDate, result.TripStartDate);
            Assert.Equal(trip.EndDate, result.TripEndDate);
            Assert.Equal(trip.StartDate, result.Date);
        }

        [Fact]
        public async Task GetAddFormAsync_TripOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "maria@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            Assert.Null(await service.GetAddFormAsync(trip.Id, stranger.Id));
        }

        [Fact]
        public async Task GetAddFormAsync_MissingTrip_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");

            Assert.Null(await service.GetAddFormAsync(999, user.Id));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(4)]
        public async Task IsDateWithinTripAsync_DateInsideTheTrip_ReturnsTrue(int dayOfTrip)
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 4);

            Assert.True(await service.IsDateWithinTripAsync(trip.Id, trip.StartDate.AddDays(dayOfTrip)));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        public async Task IsDateWithinTripAsync_DateOutsideTheTrip_ReturnsFalse(int dayOfTrip)
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 4);

            Assert.False(await service.IsDateWithinTripAsync(trip.Id, trip.StartDate.AddDays(dayOfTrip)));
        }

        [Fact]
        public async Task IsDateWithinTripAsync_LastDayWithTime_ReturnsTrue()
        {
            var user = await AddUserAsync(dbContext, "georgi@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Sozopol", 10, 2);

            Assert.True(await service.IsDateWithinTripAsync(trip.Id, trip.EndDate.AddHours(23)));
        }

        [Fact]
        public async Task IsDateWithinTripAsync_MissingTrip_ReturnsFalse()
        {
            Assert.False(await service.IsDateWithinTripAsync(999, DateTime.Today));
        }

        [Fact]
        public async Task AddAsync_OwnTrip_SavesNormalizedExpense()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Prague", 10, 3);

            var result = await service.AddAsync(new ExpenseFormViewModel
            {
                TripId = trip.Id,
                Title = "  Train   Prague -  Budapest ",
                Amount = 39.90m,
                Category = ExpenseCategory.Transport,
                Date = trip.StartDate.AddDays(1).AddHours(14)
            }, user.Id);

            Assert.True(result);
            var expense = await dbContext.Expenses.SingleAsync();
            Assert.Equal("Train Prague - Budapest", expense.Title);
            Assert.Equal(39.90m, expense.Amount);
            Assert.Equal(ExpenseCategory.Transport, expense.Category);
            Assert.Equal(trip.StartDate.AddDays(1), expense.Date);
            Assert.Equal(trip.Id, expense.TripId);
        }

        [Fact]
        public async Task AddAsync_TripOfAnotherUser_ReturnsFalseAndSavesNothing()
        {
            var owner = await AddUserAsync(dbContext, "maria@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);

            var result = await service.AddAsync(CreateForm(trip.Id, trip.StartDate), stranger.Id);

            Assert.False(result);
            Assert.Empty(dbContext.Expenses);
        }

        [Fact]
        public async Task AddAsync_MissingTrip_ReturnsFalse()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");

            Assert.False(await service.AddAsync(CreateForm(999, DateTime.Today), user.Id));
        }

        [Fact]
        public async Task GetForEditAsync_OwnExpense_ReturnsFormWithTripInfo()
        {
            var user = await AddUserAsync(dbContext, "elena@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Istanbul", -20, 3);
            var expense = await AddExpenseAsync(trip, "Bosphorus cruise", 22m, ExpenseCategory.Activities, 1);

            var result = await service.GetForEditAsync(expense.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal("Bosphorus cruise", result.Title);
            Assert.Equal(22m, result.Amount);
            Assert.Equal(ExpenseCategory.Activities, result.Category);
            Assert.Equal(trip.StartDate.AddDays(1), result.Date);
            Assert.Equal("Istanbul", result.TripTitle);
            Assert.Equal(trip.StartDate, result.TripStartDate);
            Assert.Equal(trip.EndDate, result.TripEndDate);
        }

        [Fact]
        public async Task GetForEditAsync_ExpenseOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "elena@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var expense = await AddExpenseAsync(trip, "Hotel", 234m, ExpenseCategory.Accommodation, 0);

            Assert.Null(await service.GetForEditAsync(expense.Id, stranger.Id));
        }

        [Fact]
        public async Task EditAsync_OwnExpense_UpdatesAllFields()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id, "Paris", -15, 4);
            var expense = await AddExpenseAsync(trip, "Cafe", 20m, ExpenseCategory.Food, 0);

            var result = await service.EditAsync(expense.Id, new ExpenseFormViewModel
            {
                TripId = trip.Id,
                Title = " Cafes  and bakeries ",
                Amount = 126.80m,
                Category = ExpenseCategory.Food,
                Date = trip.StartDate.AddDays(3).AddHours(9)
            }, user.Id);

            Assert.True(result);
            var updated = await dbContext.Expenses.SingleAsync();
            Assert.Equal("Cafes and bakeries", updated.Title);
            Assert.Equal(126.80m, updated.Amount);
            Assert.Equal(trip.StartDate.AddDays(3), updated.Date);
        }

        [Fact]
        public async Task EditAsync_ExpenseOfAnotherUser_ReturnsFalseAndKeepsIt()
        {
            var owner = await AddUserAsync(dbContext, "petya@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var expense = await AddExpenseAsync(trip, "Louvre tickets", 44m, ExpenseCategory.Activities, 1);

            var result = await service.EditAsync(expense.Id, CreateForm(trip.Id, trip.StartDate), stranger.Id);

            Assert.False(result);
            Assert.Equal("Louvre tickets", (await dbContext.Expenses.SingleAsync()).Title);
        }

        [Fact]
        public async Task EditAsync_MissingExpense_ReturnsFalse()
        {
            var user = await AddUserAsync(dbContext, "petya@voyage.test");

            Assert.False(await service.EditAsync(999, CreateForm(1, DateTime.Today), user.Id));
        }

        [Fact]
        public async Task GetForDeleteAsync_OwnExpense_ReturnsExpense()
        {
            var user = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var expense = await AddExpenseAsync(trip, "Bungee jump", 265m, ExpenseCategory.Activities, 2);

            var result = await service.GetForDeleteAsync(expense.Id, user.Id);

            Assert.NotNull(result);
            Assert.Equal(expense.Id, result.Id);
            Assert.Equal(trip.Id, result.TripId);
            Assert.Equal("Bungee jump", result.Title);
            Assert.Equal(265m, result.Amount);
        }

        [Fact]
        public async Task GetForDeleteAsync_ExpenseOfAnotherUser_ReturnsNull()
        {
            var owner = await AddUserAsync(dbContext, "dimitar@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var expense = await AddExpenseAsync(trip, "Bungee jump", 265m, ExpenseCategory.Activities, 2);

            Assert.Null(await service.GetForDeleteAsync(expense.Id, stranger.Id));
        }

        [Fact]
        public async Task DeleteAsync_OwnExpense_RemovesItAndReturnsTripId()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");
            var trip = await AddTripAsync(dbContext, user.Id);
            var expense = await AddExpenseAsync(trip, "Fuel", 32m, ExpenseCategory.Transport, 0);
            await AddExpenseAsync(trip, "Spa hotel", 210m, ExpenseCategory.Accommodation, 0);

            var result = await service.DeleteAsync(expense.Id, user.Id);

            Assert.Equal(trip.Id, result);
            Assert.Equal("Spa hotel", (await dbContext.Expenses.SingleAsync()).Title);
        }

        [Fact]
        public async Task DeleteAsync_ExpenseOfAnotherUser_ReturnsNullAndKeepsIt()
        {
            var owner = await AddUserAsync(dbContext, "stefan@voyage.test");
            var stranger = await AddUserAsync(dbContext, "stranger@voyage.test");
            var trip = await AddTripAsync(dbContext, owner.Id);
            var expense = await AddExpenseAsync(trip, "Fuel", 32m, ExpenseCategory.Transport, 0);

            var result = await service.DeleteAsync(expense.Id, stranger.Id);

            Assert.Null(result);
            Assert.Single(dbContext.Expenses);
        }

        [Fact]
        public async Task DeleteAsync_MissingExpense_ReturnsNull()
        {
            var user = await AddUserAsync(dbContext, "stefan@voyage.test");

            Assert.Null(await service.DeleteAsync(999, user.Id));
        }

        private async Task<Expense> AddExpenseAsync(
            Trip trip,
            string title,
            decimal amount,
            ExpenseCategory category,
            int dayOfTrip)
        {
            var expense = new Expense
            {
                TripId = trip.Id,
                Title = title,
                Amount = amount,
                Category = category,
                Date = trip.StartDate.AddDays(dayOfTrip)
            };

            dbContext.Expenses.Add(expense);
            await dbContext.SaveChangesAsync();

            return expense;
        }

        private static ExpenseFormViewModel CreateForm(int tripId, DateTime date)
        {
            return new ExpenseFormViewModel
            {
                TripId = tripId,
                Title = "Changed title",
                Amount = 10m,
                Category = ExpenseCategory.Other,
                Date = date
            };
        }
    }
}
