using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants.Validation;
using Voyage.Web.Common;
using Voyage.Web.Data;
using Voyage.Web.Data.Entities;
using Voyage.Web.Extensions;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Packing;

namespace Voyage.Web.Services
{
    public class PackingService : IPackingService
    {
        private readonly ApplicationDbContext dbContext;

        public PackingService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PackingListViewModel?> GetListAsync(
            int tripId,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var trip = await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.Id == tripId && t.OwnerId == userId)
                .Select(t => new
                {
                    t.Title,
                    t.StartDate
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return null;
            }

            var items = await dbContext.PackingItems
                .AsNoTracking()
                .Where(p => p.TripId == tripId)
                .OrderBy(p => p.IsPacked)
                .ThenBy(p => p.Name)
                .Select(p => new PackingItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Category = p.Category,
                    IsPacked = p.IsPacked
                })
                .ToListAsync();

            var groups = items
                .GroupBy(p => p.Category)
                .OrderBy(g => g.Key)
                .Select(g => new PackingCategoryGroupViewModel
                {
                    Category = g.Key,
                    Items = g.ToList(),
                    PackedCount = g.Count(p => p.IsPacked),
                    TotalCount = g.Count()
                })
                .ToList();

            var packedCount = items.Count(p => p.IsPacked);

            return new PackingListViewModel
            {
                TripId = tripId,
                TripName = trip.Title,
                StartDate = trip.StartDate,
                Groups = groups,
                Progress = new PackingProgressViewModel
                {
                    Total = items.Count,
                    Packed = packedCount
                },
                MaxItems = PackingItemValidationConstants.MaxPerTrip
            };
        }

        public async Task<PackingProgressViewModel> GetProgressAsync(int tripId)
        {
            var items = dbContext.PackingItems
                .AsNoTracking()
                .Where(p => p.TripId == tripId);

            var total = await items.CountAsync();
            var packed = await items.CountAsync(p => p.IsPacked);

            return new PackingProgressViewModel
            {
                Total = total,
                Packed = packed
            };
        }

        public async Task<PackingResult> AddAsync(
            int tripId,
            PackingItemFormModel model,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            if (tripId <= 0 || !await IsTripOwnerAsync(tripId, userId))
            {
                return PackingResult.TripNotFound;
            }

            if (!Enum.IsDefined(typeof(PackingCategory), model.Category))
            {
                return PackingResult.InvalidCategory;
            }

            var normalizedName = NormalizeName(model.Name);

            await using var transaction = await dbContext.Database
                .BeginLockedTransactionAsync(TripLockResource(tripId));

            if (await IsDuplicateAsync(tripId, normalizedName))
            {
                return PackingResult.Duplicate;
            }

            var itemsCount = await dbContext.PackingItems
                .CountAsync(p => p.TripId == tripId);

            if (itemsCount >= PackingItemValidationConstants.MaxPerTrip)
            {
                return PackingResult.LimitReached;
            }

            var item = new PackingItem
            {
                TripId = tripId,
                Name = normalizedName,
                Quantity = model.Quantity,
                Category = model.Category,
                IsPacked = false,
                CreatedOn = DateTime.UtcNow
            };

            await dbContext.PackingItems.AddAsync(item);

            var saved = await TrySaveAsync(item);

            if (saved && transaction != null)
            {
                await transaction.CommitAsync();
            }

            return saved
                ? PackingResult.Success
                : PackingResult.Duplicate;
        }

        public async Task<PackingItemEditViewModel?> GetForEditAsync(
            int id,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            return await dbContext.PackingItems
                .AsNoTracking()
                .Where(p => p.Id == id && p.Trip.OwnerId == userId)
                .Select(p => new PackingItemEditViewModel
                {
                    Id = p.Id,
                    TripId = p.TripId,
                    TripName = p.Trip.Title,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    Category = p.Category
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PackingOperationResult> EditAsync(
            int id,
            PackingItemFormModel model,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var item = await FindOwnedItemAsync(id, userId);

            if (item == null)
            {
                return PackingOperationResult.Fail(PackingResult.ItemNotFound);
            }

            if (!Enum.IsDefined(typeof(PackingCategory), model.Category))
            {
                return PackingOperationResult.Fail(PackingResult.InvalidCategory);
            }

            var normalizedName = NormalizeName(model.Name);

            if (await IsDuplicateAsync(item.TripId, normalizedName, item.Id))
            {
                return PackingOperationResult.Fail(PackingResult.Duplicate);
            }

            item.Name = normalizedName;
            item.Quantity = model.Quantity;
            item.Category = model.Category;

            var saved = await TrySaveAsync(item);

            if (!saved)
            {
                return PackingOperationResult.Fail(PackingResult.Duplicate);
            }

            return new PackingOperationResult
            {
                Result = PackingResult.Success,
                TripId = item.TripId,
                IsPacked = item.IsPacked
            };
        }

        public async Task<PackingOperationResult> DeleteAsync(
            int id,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var item = await FindOwnedItemAsync(id, userId);

            if (item == null)
            {
                return PackingOperationResult.Fail(PackingResult.ItemNotFound);
            }

            var tripId = item.TripId;

            dbContext.PackingItems.Remove(item);
            await dbContext.SaveChangesAsync();

            return new PackingOperationResult
            {
                Result = PackingResult.Success,
                TripId = tripId
            };
        }

        public async Task<PackingOperationResult> TogglePackedAsync(
            int id,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var item = await FindOwnedItemAsync(id, userId);

            if (item == null)
            {
                return PackingOperationResult.Fail(PackingResult.ItemNotFound);
            }

            item.IsPacked = !item.IsPacked;

            await dbContext.SaveChangesAsync();

            return new PackingOperationResult
            {
                Result = PackingResult.Success,
                TripId = item.TripId,
                IsPacked = item.IsPacked
            };
        }

        public async Task<PackingBulkResult> AddEssentialsAsync(
            int tripId,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var trip = await dbContext.Trips
                .AsNoTracking()
                .Where(t => t.Id == tripId && t.OwnerId == userId)
                .Select(t => new
                {
                    t.StartDate,
                    t.EndDate
                })
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return PackingBulkResult.Fail(PackingResult.TripNotFound);
            }

            await using var transaction = await dbContext.Database
                .BeginLockedTransactionAsync(TripLockResource(tripId));

            var existingNames = (await dbContext.PackingItems
                    .Where(p => p.TripId == tripId)
                    .Select(p => p.Name)
                    .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            int capacity = PackingItemValidationConstants.MaxPerTrip
                - existingNames.Count;

            if (capacity <= 0)
            {
                return PackingBulkResult.Fail(PackingResult.LimitReached);
            }

            int tripDays = (trip.EndDate.Date - trip.StartDate.Date).Days + 1;

            var missing = PackingTemplates.GetEssentials(tripDays)
                .Where(t => !existingNames.Contains(t.Name))
                .ToList();

            var toAdd = missing.Take(capacity).ToList();

            if (toAdd.Count == 0)
            {
                return new PackingBulkResult
                {
                    Result = PackingResult.Success,
                    Affected = 0
                };
            }

            var now = DateTime.UtcNow;

            await dbContext.PackingItems.AddRangeAsync(
                toAdd.Select(t => new PackingItem
                {
                    TripId = tripId,
                    Name = t.Name,
                    Category = t.Category,
                    Quantity = t.Quantity,
                    IsPacked = false,
                    CreatedOn = now
                }));

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException { Number: 2627 or 2601 })
            {
                dbContext.ChangeTracker.Clear();

                return PackingBulkResult.Fail(PackingResult.Duplicate);
            }

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return new PackingBulkResult
            {
                Result = PackingResult.Success,
                Affected = toAdd.Count,
                NotAddedDueToLimit = missing.Count - toAdd.Count
            };
        }

        public async Task<PackingBulkResult> UnpackAllAsync(
            int tripId,
            string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            if (!await IsTripOwnerAsync(tripId, userId))
            {
                return PackingBulkResult.Fail(PackingResult.TripNotFound);
            }

            int affected = await dbContext.PackingItems
                .Where(p => p.TripId == tripId && p.IsPacked)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    p => p.IsPacked,
                    false));

            return new PackingBulkResult
            {
                Result = PackingResult.Success,
                Affected = affected
            };
        }

        private static string TripLockResource(int tripId)
        {
            return $"packing-trip:{tripId}";
        }

        private async Task<bool> IsTripOwnerAsync(int tripId, string userId)
        {
            return await dbContext.Trips
                .AnyAsync(t => t.Id == tripId && t.OwnerId == userId);
        }

        private async Task<PackingItem?> FindOwnedItemAsync(int id, string userId)
        {
            return await dbContext.PackingItems
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.Trip.OwnerId == userId);
        }

        private async Task<bool> IsDuplicateAsync(
            int tripId,
            string name,
            int? excludedItemId = null)
        {
            var query = dbContext.PackingItems
                .Where(p =>
                    p.TripId == tripId &&
                    p.Name == name);

            if (excludedItemId.HasValue)
            {
                query = query.Where(p => p.Id != excludedItemId.Value);
            }

            return await query.AnyAsync();
        }

        private static string NormalizeName(string name)
        {
            return Regex.Replace(name.Trim(), @"\s+", " ");
        }

        private async Task<bool> TrySaveAsync(PackingItem item)
        {
            try
            {
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException { Number: 2627 or 2601 })
            {
                dbContext.Entry(item).State = EntityState.Detached;
                return false;
            }
        }
    }
}