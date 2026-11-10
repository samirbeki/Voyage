using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data;
using Voyage.Web.Data.Entities;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Destinations;

namespace Voyage.Web.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly ApplicationDbContext dbContext;

        public DestinationService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<DestinationAllViewModel>> GetAllAsync()
        {
            return await dbContext.Destinations
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new DestinationAllViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Country = d.Country,
                    ImageUrl = d.ImageUrl,
                    CategoryName = d.Category.Name,
                    Description = d.Description
                })
                .ToListAsync();
        }
        

        public async Task<DestinationDetailsViewModel?> GetDetailsAsync(int id)
        {
            return await dbContext.Destinations
                .AsNoTracking()
                .Where(d => d.Id == id && !d.IsDeleted)
                .Select(d => new DestinationDetailsViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Country = d.Country,
                    ImageUrl = d.ImageUrl,
                    CategoryName = d.Category.Name,
                    Description = d.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CategoryOptionViewModel>> GetCategoriesAsync()
        {
            return await dbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryOptionViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task AddAsync(DestinationFormViewModel model)
        {
            var destination = new Destination
            {
                Name = model.Name,
                Country = model.Country,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId
            };

            await dbContext.Destinations.AddAsync(destination);

            await dbContext.SaveChangesAsync();
        }

        public async Task<DestinationFormViewModel?> GetForEditAsync(int id)
        {
            return await dbContext.Destinations
                .AsNoTracking()
                .Where(d => d.Id == id && !d.IsDeleted)
                .Select(d => new DestinationFormViewModel
                {
                    Name = d.Name,
                    Country = d.Country,
                    Description = d.Description,
                    ImageUrl = d.ImageUrl,
                    CategoryId = d.CategoryId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EditAsync(int id, DestinationFormViewModel model)
        {
            var destination = await dbContext.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (destination == null)
            {
                return false;
            }

            destination.Name = model.Name;
            destination.Country = model.Country;
            destination.Description = model.Description;
            destination.ImageUrl = model.ImageUrl;
            destination.CategoryId = model.CategoryId;

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}