using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Common.Utilities;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Destinations;
using Voyage.Web.ViewModels.Enums;

namespace Voyage.Web.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly ApplicationDbContext dbContext;

        public DestinationService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<DestinationsIndexViewModel> GetAllAsync(
            string? searchTerm,
            int? categoryId,
            int currentPage,
            DestinationSorting sorting)
        {
            IQueryable<Destination> query = dbContext.Destinations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(d =>
                    d.Name.Contains(searchTerm) ||
                    d.Country.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)PagingConstants.DestinationsPerPage);

            if (currentPage < 1)
            {
                currentPage = 1;
            }

            if (totalPages > 0 && currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            query = sorting switch
            {
                DestinationSorting.NameDesc => query.OrderByDescending(d => d.Name),
                DestinationSorting.Popular => query
                    .OrderByDescending(d => d.TripDestinations.Count)
                    .ThenBy(d => d.Name),
                DestinationSorting.HighestRated => query
                    .OrderByDescending(d => d.Reviews
                        .Where(r => !r.IsHidden)
                        .Average(r => (double?)r.Rating) ?? 0)
                    .ThenByDescending(d => d.Reviews.Count(r => !r.IsHidden))
                    .ThenBy(d => d.Name),
                _ => query.OrderBy(d => d.Name)
            };

            var destinations = await query
                .Skip((currentPage - 1) * PagingConstants.DestinationsPerPage)
                .Take(PagingConstants.DestinationsPerPage)
                .Select(DestinationProjections.ToCard)
                .ToListAsync();

            return new DestinationsIndexViewModel
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                Sorting = sorting,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Destinations = destinations,
                Categories = await GetCategoriesAsync()
            };
        }

        public async Task<DestinationDetailsViewModel?> GetDetailsAsync(int id)
        {
            var destination = await dbContext.Destinations
                .AsNoTracking()
                .Where(d => d.Id == id && !d.IsDeleted)
                .Select(d => new DestinationDetailsViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Country = d.Country,
                    ImageUrl = d.ImageUrl,
                    CategoryName = d.Category.Name,
                    CategoryId = d.CategoryId,
                    Description = d.Description
                })
                .FirstOrDefaultAsync();

            if (destination == null)
            {
                return null;
            }

            destination.SimilarDestinations = await dbContext.Destinations
                .AsNoTracking()
                .Where(d =>
                    d.CategoryId == destination.CategoryId &&
                    d.Id != destination.Id)
                .OrderBy(d => d.Name)
                .Take(PagingConstants.SimilarDestinationsCount)
                .Select(DestinationProjections.ToCard)
                .ToListAsync();

            return destination;
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
                Name = TextNormalizer.SingleLine(model.Name),
                Country = TextNormalizer.SingleLine(model.Country),
                Description = model.Description.Trim(),
                ImageUrl = model.ImageUrl?.Trim(),
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

            destination.Name = TextNormalizer.SingleLine(model.Name);
            destination.Country = TextNormalizer.SingleLine(model.Country);
            destination.Description = model.Description.Trim();
            destination.ImageUrl = model.ImageUrl?.Trim();
            destination.CategoryId = model.CategoryId;

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var destination = await dbContext.Destinations
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (destination == null)
            {
                return false;
            }

            destination.IsDeleted = true;

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await dbContext.Categories
                .AnyAsync(c => c.Id == categoryId);
        }

        public async Task<bool> DestinationExistsAsync(
            string name,
            string country,
            int? excludeId = null)
        {
            name = TextNormalizer.SingleLine(name);
            country = TextNormalizer.SingleLine(country);

            return await dbContext.Destinations
                .AnyAsync(d =>
                    !d.IsDeleted &&
                    d.Name == name &&
                    d.Country == country &&
                    (excludeId == null || d.Id != excludeId));
        }
    }
}