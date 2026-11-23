using Microsoft.EntityFrameworkCore;
using Voyage.Common.Utilities;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Contracts;
using Voyage.Web.ViewModels.Categories;

namespace Voyage.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext dbContext;

        public CategoryService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<CategoryAllViewModel>> GetAllAsync()
        {
            return await dbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryAllViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DestinationsCount = c.Destinations.Count(d => !d.IsDeleted)
                })
                .ToListAsync();
        }

        public async Task<CategoryAllViewModel?> GetByIdAsync(int id)
        {
            return await dbContext.Categories
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoryAllViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DestinationsCount = c.Destinations.Count(d => !d.IsDeleted)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CategoryFormViewModel?> GetForEditAsync(int id)
        {
            return await dbContext.Categories
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoryFormViewModel
                {
                    Name = c.Name,
                    Description = c.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(CategoryFormViewModel model)
        {
            var category = new Category
            {
                Name = TextNormalizer.SingleLine(model.Name),
                Description = model.Description?.Trim()
            };

            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> EditAsync(int id, CategoryFormViewModel model)
        {
            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false;
            }

            category.Name = TextNormalizer.SingleLine(model.Name);
            category.Description = model.Description?.Trim();

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false;
            }

            var deletedDestinations = await dbContext.Destinations
                .IgnoreQueryFilters()
                .Where(d => d.CategoryId == id && d.IsDeleted)
                .ToListAsync();

            var deletedIds = deletedDestinations
                .Select(d => d.Id)
                .ToList();

            var tripDestinations = await dbContext.TripDestinations
                .IgnoreQueryFilters()
                .Where(td => deletedIds.Contains(td.DestinationId))
                .ToListAsync();

            var reviews = await dbContext.Reviews
                .IgnoreQueryFilters()
                .Where(r => deletedIds.Contains(r.DestinationId))
                .ToListAsync();

            var favorites = await dbContext.Favorites
                .IgnoreQueryFilters()
                .Where(f => deletedIds.Contains(f.DestinationId))
                .ToListAsync();

            dbContext.Reviews.RemoveRange(reviews);
            dbContext.Favorites.RemoveRange(favorites);
            dbContext.TripDestinations.RemoveRange(tripDestinations);
            dbContext.Destinations.RemoveRange(deletedDestinations);

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            name = TextNormalizer.SingleLine(name);

            return await dbContext.Categories
                .AnyAsync(c => c.Name == name
                    && (excludeId == null || c.Id != excludeId));
        }

        public async Task<bool> HasDestinationsAsync(int id)
        {
            return await dbContext.Destinations
                .AnyAsync(d => d.CategoryId == id);
        }
    }
}
