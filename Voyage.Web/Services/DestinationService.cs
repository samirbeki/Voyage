using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data;
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
    }
}