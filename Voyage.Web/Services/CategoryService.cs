using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data;
using Voyage.Web.Data.Entities;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Categories;

namespace Voyage.Web.Services
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
                Name = model.Name.Trim(),
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

            category.Name = model.Name.Trim();
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

            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            name = name.Trim();

            return await dbContext.Categories
                .AnyAsync(c => c.Name == name
                    && (excludeId == null || c.Id != excludeId));
        }

        public async Task<bool> HasDestinationsAsync(int id)
        {
            return await dbContext.Destinations
                .IgnoreQueryFilters()
                .AnyAsync(d => d.CategoryId == id);
        }
    }
}