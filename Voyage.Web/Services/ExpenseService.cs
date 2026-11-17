using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data;
using Voyage.Web.Data.Entities;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Expenses;

namespace Voyage.Web.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext dbContext;

        public ExpenseService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ExpenseFormViewModel?> GetAddFormAsync(int tripId, string userId)
        {
            return await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.Id == tripId && t.OwnerId == userId)
                .Select(t => new ExpenseFormViewModel
                {
                    TripId = t.Id,
                    TripTitle = t.Title,
                    TripStartDate = t.StartDate,
                    TripEndDate = t.EndDate,
                    Date = t.StartDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDateWithinTripAsync(int tripId, DateTime date)
        {
            return await dbContext.Trips
                .AnyAsync(t =>
                    t.Id == tripId &&
                    date.Date >= t.StartDate.Date &&
                    date.Date <= t.EndDate.Date);
        }

        public async Task<bool> AddAsync(ExpenseFormViewModel model, string userId)
        {
            var tripExists = await dbContext.Trips
                .AnyAsync(t => t.Id == model.TripId && t.OwnerId == userId);

            if (!tripExists)
            {
                return false;
            }

            var expense = new Expense
            {
                Title = model.Title.Trim(),
                Amount = model.Amount,
                Category = model.Category,
                Date = model.Date.Date,
                TripId = model.TripId
            };

            await dbContext.Expenses.AddAsync(expense);
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ExpenseFormViewModel?> GetForEditAsync(int id, string userId)
        {
            return await dbContext.Expenses
                .AsNoTracking()
                .Where(e => e.Id == id && e.Trip.OwnerId == userId)
                .Select(e => new ExpenseFormViewModel
                {
                    Title = e.Title,
                    Amount = e.Amount,
                    Category = e.Category,
                    Date = e.Date,
                    TripId = e.TripId,
                    TripTitle = e.Trip.Title,
                    TripStartDate = e.Trip.StartDate,
                    TripEndDate = e.Trip.EndDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EditAsync(int id, ExpenseFormViewModel model, string userId)
        {
            var expense = await dbContext.Expenses
                .FirstOrDefaultAsync(e =>
                    e.Id == id && e.Trip.OwnerId == userId);

            if (expense == null)
            {
                return false;
            }

            expense.Title = model.Title.Trim();
            expense.Amount = model.Amount;
            expense.Category = model.Category;
            expense.Date = model.Date.Date;

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ExpenseViewModel?> GetForDeleteAsync(int id, string userId)
        {
            return await dbContext.Expenses
                .AsNoTracking()
                .Where(e => e.Id == id && e.Trip.OwnerId == userId)
                .Select(e => new ExpenseViewModel
                {
                    Id = e.Id,
                    TripId = e.TripId,
                    Title = e.Title,
                    Amount = e.Amount,
                    Category = e.Category,
                    Date = e.Date
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int?> DeleteAsync(int id, string userId)
        {
            var expense = await dbContext.Expenses
                .FirstOrDefaultAsync(e =>
                    e.Id == id && e.Trip.OwnerId == userId);

            if (expense == null)
            {
                return null;
            }

            var tripId = expense.TripId;

            dbContext.Expenses.Remove(expense);

            await dbContext.SaveChangesAsync();

            return tripId;
        }
    }
}