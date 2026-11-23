using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Categories;
using Xunit;

namespace Voyage.Services.Tests
{
    public class CategoryServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly CategoryService service;

        public CategoryServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new CategoryService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCategoriesOrderedByName()
        {
            await AddCategoryAsync("Mountains");
            await AddCategoryAsync("Beaches");
            await AddCategoryAsync("Culture");

            var result = (await service.GetAllAsync()).ToList();

            Assert.Equal(new[] { "Beaches", "Culture", "Mountains" }, result.Select(c => c.Name));
        }

        [Fact]
        public async Task GetAllAsync_CountsOnlyDestinationsThatAreNotDeleted()
        {
            var category = await AddCategoryAsync("Mountains");
            await AddDestinationAsync(category.Id, "Bansko");
            await AddDestinationAsync(category.Id, "Borovets");
            await AddDestinationAsync(category.Id, "Pamporovo", isDeleted: true);

            var result = (await service.GetAllAsync()).Single();

            Assert.Equal(2, result.DestinationsCount);
        }

        [Fact]
        public async Task GetAllAsync_NoCategories_ReturnsEmptyCollection()
        {
            var result = await service.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsCategory()
        {
            var category = await AddCategoryAsync("Culture", "Museums and old towns");
            await AddDestinationAsync(category.Id, "Rome");

            var result = await service.GetByIdAsync(category.Id);

            Assert.NotNull(result);
            Assert.Equal("Culture", result.Name);
            Assert.Equal("Museums and old towns", result.Description);
            Assert.Equal(1, result.DestinationsCount);
        }

        [Fact]
        public async Task GetByIdAsync_MissingId_ReturnsNull()
        {
            var result = await service.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetForEditAsync_ExistingId_ReturnsFormModel()
        {
            var category = await AddCategoryAsync("Wellness", "Spa and relax");

            var result = await service.GetForEditAsync(category.Id);

            Assert.NotNull(result);
            Assert.Equal("Wellness", result.Name);
            Assert.Equal("Spa and relax", result.Description);
        }

        [Fact]
        public async Task GetForEditAsync_MissingId_ReturnsNull()
        {
            var result = await service.GetForEditAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_NormalizesNameAndTrimsDescription()
        {
            var model = new CategoryFormViewModel
            {
                Name = "  City   Breaks ",
                Description = "  Short trips to big cities  "
            };

            await service.AddAsync(model);

            var category = await dbContext.Categories.SingleAsync();
            Assert.Equal("City Breaks", category.Name);
            Assert.Equal("Short trips to big cities", category.Description);
        }

        [Fact]
        public async Task AddAsync_EmptyDescription_SavesNullDescription()
        {
            await service.AddAsync(new CategoryFormViewModel { Name = "Islands" });

            var category = await dbContext.Categories.SingleAsync();
            Assert.Null(category.Description);
        }

        [Fact]
        public async Task EditAsync_ExistingId_UpdatesCategoryAndReturnsTrue()
        {
            var category = await AddCategoryAsync("Mountain", "Old text");

            var result = await service.EditAsync(category.Id, new CategoryFormViewModel
            {
                Name = " Mountains ",
                Description = " Hiking and skiing "
            });

            Assert.True(result);
            var updated = await dbContext.Categories.SingleAsync();
            Assert.Equal("Mountains", updated.Name);
            Assert.Equal("Hiking and skiing", updated.Description);
        }

        [Fact]
        public async Task EditAsync_MissingId_ReturnsFalse()
        {
            var result = await service.EditAsync(999, new CategoryFormViewModel { Name = "Beaches" });

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_MissingId_ReturnsFalse()
        {
            var result = await service.DeleteAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_EmptyCategory_RemovesCategory()
        {
            var category = await AddCategoryAsync("Deserts");

            var result = await service.DeleteAsync(category.Id);

            Assert.True(result);
            Assert.Empty(dbContext.Categories);
        }

        [Fact]
        public async Task DeleteAsync_WithDeletedDestinations_RemovesThemAndTheirRelatedData()
        {
            var category = await AddCategoryAsync("Lakes");
            var destination = await AddDestinationAsync(category.Id, "Ohrid", isDeleted: true);

            var user = new IdentityUser { Id = "user-1", UserName = "maria@voyage.test" };
            dbContext.Users.Add(user);

            var trip = new Trip
            {
                Title = "Lake weekend",
                StartDate = DateTime.Today.AddDays(10),
                EndDate = DateTime.Today.AddDays(12),
                Budget = 300m,
                OwnerId = user.Id
            };
            trip.TripDestinations.Add(new TripDestination { DestinationId = destination.Id });
            dbContext.Trips.Add(trip);

            dbContext.Reviews.Add(new Review
            {
                AuthorId = user.Id,
                DestinationId = destination.Id,
                Rating = 5,
                Comment = "Calm water and great fish."
            });
            dbContext.Favorites.Add(new Favorite { UserId = user.Id, DestinationId = destination.Id });
            await dbContext.SaveChangesAsync();

            var result = await service.DeleteAsync(category.Id);

            Assert.True(result);
            Assert.Empty(dbContext.Categories);
            Assert.Empty(dbContext.Destinations.IgnoreQueryFilters());
            Assert.Empty(dbContext.Reviews.IgnoreQueryFilters());
            Assert.Empty(dbContext.Favorites.IgnoreQueryFilters());
            Assert.Empty(dbContext.TripDestinations.IgnoreQueryFilters());
            Assert.Single(dbContext.Trips);
        }

        [Fact]
        public async Task ExistsByNameAsync_SameNameWithExtraSpaces_ReturnsTrue()
        {
            await AddCategoryAsync("City Breaks");

            var result = await service.ExistsByNameAsync("  City   Breaks ");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByNameAsync_UnknownName_ReturnsFalse()
        {
            await AddCategoryAsync("Beaches");

            var result = await service.ExistsByNameAsync("Mountains");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByNameAsync_ExcludedCategory_ReturnsFalse()
        {
            var category = await AddCategoryAsync("Beaches");

            var result = await service.ExistsByNameAsync("Beaches", category.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task HasDestinationsAsync_WithActiveDestination_ReturnsTrue()
        {
            var category = await AddCategoryAsync("Culture");
            await AddDestinationAsync(category.Id, "Prague");

            var result = await service.HasDestinationsAsync(category.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task HasDestinationsAsync_WithOnlyDeletedDestinations_ReturnsFalse()
        {
            var category = await AddCategoryAsync("Culture");
            await AddDestinationAsync(category.Id, "Old Town", isDeleted: true);

            var result = await service.HasDestinationsAsync(category.Id);

            Assert.False(result);
        }

        private async Task<Category> AddCategoryAsync(string name, string? description = null)
        {
            var category = new Category { Name = name, Description = description };

            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            return category;
        }

        private async Task<Destination> AddDestinationAsync(int categoryId, string name, bool isDeleted = false)
        {
            var destination = new Destination
            {
                Name = name,
                Country = "Bulgaria",
                Description = "A destination used in tests.",
                CategoryId = categoryId,
                IsDeleted = isDeleted
            };

            dbContext.Destinations.Add(destination);
            await dbContext.SaveChangesAsync();

            return destination;
        }
    }
}
