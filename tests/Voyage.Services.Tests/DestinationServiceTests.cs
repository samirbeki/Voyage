using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Destinations;
using Voyage.Web.ViewModels.Enums;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class DestinationServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly DestinationService service;

        public DestinationServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new DestinationService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetAllAsync_NoFilters_ReturnsDestinationsSortedByName()
        {
            var category = await AddCategoryAsync(dbContext, "Mountains");
            await AddDestinationAsync(dbContext, category.Id, "Zermatt", "Switzerland");
            await AddDestinationAsync(dbContext, category.Id, "Bansko");
            await AddDestinationAsync(dbContext, category.Id, "Interlaken", "Switzerland");

            var result = await service.GetAllAsync(null, null, 1, DestinationSorting.NameAsc);

            Assert.Equal(new[] { "Bansko", "Interlaken", "Zermatt" }, result.Destinations.Select(d => d.Name));
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public async Task GetAllAsync_SearchByName_ReturnsOnlyMatchingDestinations()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            await AddDestinationAsync(dbContext, category.Id, "Prague", "Czech Republic");

            var result = await service.GetAllAsync("Rom", null, 1, DestinationSorting.NameAsc);

            Assert.Equal("Rome", Assert.Single(result.Destinations).Name);
        }

        [Fact]
        public async Task GetAllAsync_SearchByCountry_ReturnsOnlyMatchingDestinations()
        {
            var category = await AddCategoryAsync(dbContext, "Mountains");
            await AddDestinationAsync(dbContext, category.Id, "Zermatt", "Switzerland");
            await AddDestinationAsync(dbContext, category.Id, "Interlaken", "Switzerland");
            await AddDestinationAsync(dbContext, category.Id, "Bansko", "Bulgaria");

            var result = await service.GetAllAsync("Switzerland", null, 1, DestinationSorting.NameAsc);

            Assert.Equal(new[] { "Interlaken", "Zermatt" }, result.Destinations.Select(d => d.Name));
        }

        [Fact]
        public async Task GetAllAsync_SearchTermWithSpaces_IsTrimmed()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, category.Id, "Paris", "France");

            var result = await service.GetAllAsync("  Paris  ", null, 1, DestinationSorting.NameAsc);

            Assert.Equal("Paris", result.SearchTerm);
            Assert.Single(result.Destinations);
        }

        [Fact]
        public async Task GetAllAsync_FilterByCategory_ReturnsOnlyThatCategory()
        {
            var beaches = await AddCategoryAsync(dbContext, "Beaches");
            var culture = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, beaches.Id, "Sozopol");
            await AddDestinationAsync(dbContext, culture.Id, "Plovdiv");

            var result = await service.GetAllAsync(null, beaches.Id, 1, DestinationSorting.NameAsc);

            Assert.Equal("Sozopol", Assert.Single(result.Destinations).Name);
            Assert.Equal(beaches.Id, result.CategoryId);
        }

        [Fact]
        public async Task GetAllAsync_SearchAndCategory_ApplyBothFilters()
        {
            var beaches = await AddCategoryAsync(dbContext, "Beaches");
            var culture = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, beaches.Id, "Sozopol");
            await AddDestinationAsync(dbContext, beaches.Id, "Santorini", "Greece");
            await AddDestinationAsync(dbContext, culture.Id, "Sofia");

            var result = await service.GetAllAsync("So", beaches.Id, 1, DestinationSorting.NameAsc);

            Assert.Equal("Sozopol", Assert.Single(result.Destinations).Name);
        }

        [Fact]
        public async Task GetAllAsync_DeletedDestinations_AreNotReturned()
        {
            var category = await AddCategoryAsync(dbContext, "Lakes");
            await AddDestinationAsync(dbContext, category.Id, "Rila Lakes");
            await AddDestinationAsync(dbContext, category.Id, "Ohrid", "North Macedonia", isDeleted: true);

            var result = await service.GetAllAsync(null, null, 1, DestinationSorting.NameAsc);

            Assert.Equal("Rila Lakes", Assert.Single(result.Destinations).Name);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetAllAsync_NameDescending_ReturnsReverseAlphabeticalOrder()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, category.Id, "Budapest", "Hungary");
            await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            await AddDestinationAsync(dbContext, category.Id, "Prague", "Czech Republic");

            var result = await service.GetAllAsync(null, null, 1, DestinationSorting.NameDesc);

            Assert.Equal(new[] { "Rome", "Prague", "Budapest" }, result.Destinations.Select(d => d.Name));
        }

        [Fact]
        public async Task GetAllAsync_Popular_OrdersByNumberOfTripsThenByName()
        {
            var user = await AddUserAsync(dbContext, "traveler@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");
            var berlin = await AddDestinationAsync(dbContext, category.Id, "Berlin", "Germany");
            var athens = await AddDestinationAsync(dbContext, category.Id, "Athens", "Greece");

            await AddTripAsync(dbContext, user.Id, "Italy 1", destinationIds: rome.Id);
            await AddTripAsync(dbContext, user.Id, "Italy 2", destinationIds: rome.Id);
            await AddTripAsync(dbContext, user.Id, "Europe", destinationIds: new[] { rome.Id, paris.Id, berlin.Id });

            var result = await service.GetAllAsync(null, null, 1, DestinationSorting.Popular);

            Assert.Equal(new[] { "Rome", "Berlin", "Paris", "Athens" }, result.Destinations.Select(d => d.Name));
        }

        [Fact]
        public async Task GetAllAsync_HighestRated_IgnoresHiddenReviewsAndUsesReviewCountAsTieBreaker()
        {
            var user1 = await AddUserAsync(dbContext, "maria@voyage.test");
            var user2 = await AddUserAsync(dbContext, "georgi@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Nature");
            var banff = await AddDestinationAsync(dbContext, category.Id, "Banff", "Canada");
            var plitvice = await AddDestinationAsync(dbContext, category.Id, "Plitvice Lakes", "Croatia");
            var bali = await AddDestinationAsync(dbContext, category.Id, "Bali", "Indonesia");
            var akureyri = await AddDestinationAsync(dbContext, category.Id, "Akureyri", "Iceland");

            await AddReviewAsync(dbContext, user1.Id, banff.Id, 5);
            await AddReviewAsync(dbContext, user1.Id, plitvice.Id, 5);
            await AddReviewAsync(dbContext, user2.Id, plitvice.Id, 5);
            await AddReviewAsync(dbContext, user1.Id, bali.Id, 3);
            await AddReviewAsync(dbContext, user2.Id, bali.Id, 5, isHidden: true);

            var result = await service.GetAllAsync(null, null, 1, DestinationSorting.HighestRated);

            Assert.Equal(new[] { "Plitvice Lakes", "Banff", "Bali", "Akureyri" }, result.Destinations.Select(d => d.Name));
        }

        [Fact]
        public async Task GetAllAsync_SecondPage_ReturnsRemainingDestinations()
        {
            var category = await AddCategoryAsync(dbContext, "Cities");
            for (var i = 1; i <= PagingConstants.DestinationsPerPage + 1; i++)
            {
                await AddDestinationAsync(dbContext, category.Id, $"City {i:D2}");
            }

            var result = await service.GetAllAsync(null, null, 2, DestinationSorting.NameAsc);

            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(PagingConstants.DestinationsPerPage + 1, result.TotalCount);
            Assert.Equal("City 13", Assert.Single(result.Destinations).Name);
        }

        [Fact]
        public async Task GetAllAsync_PageBelowOne_ReturnsFirstPage()
        {
            var category = await AddCategoryAsync(dbContext, "Cities");
            await AddDestinationAsync(dbContext, category.Id, "Sofia");

            var result = await service.GetAllAsync(null, null, -5, DestinationSorting.NameAsc);

            Assert.Equal(1, result.CurrentPage);
            Assert.Single(result.Destinations);
        }

        [Fact]
        public async Task GetAllAsync_PageAboveLast_ReturnsLastPage()
        {
            var category = await AddCategoryAsync(dbContext, "Cities");
            for (var i = 1; i <= PagingConstants.DestinationsPerPage + 2; i++)
            {
                await AddDestinationAsync(dbContext, category.Id, $"City {i:D2}");
            }

            var result = await service.GetAllAsync(null, null, 99, DestinationSorting.NameAsc);

            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(2, result.Destinations.Count());
        }

        [Fact]
        public async Task GetAllAsync_NoMatches_ReturnsEmptyResult()
        {
            var category = await AddCategoryAsync(dbContext, "Cities");
            await AddDestinationAsync(dbContext, category.Id, "Sofia");

            var result = await service.GetAllAsync("Atlantis", null, 1, DestinationSorting.NameAsc);

            Assert.Empty(result.Destinations);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages);
            Assert.Equal(1, result.CurrentPage);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCategoriesForTheFilterOrderedByName()
        {
            await AddCategoryAsync(dbContext, "Wellness");
            await AddCategoryAsync(dbContext, "Beaches");

            var result = await service.GetAllAsync(null, null, 1, DestinationSorting.NameAsc);

            Assert.Equal(new[] { "Beaches", "Wellness" }, result.Categories.Select(c => c.Name));
        }

        [Fact]
        public async Task GetAllAsync_Card_ContainsRatingAndReviewsCountWithoutHiddenReviews()
        {
            var user1 = await AddUserAsync(dbContext, "ivan@voyage.test");
            var user2 = await AddUserAsync(dbContext, "elena@voyage.test");
            var user3 = await AddUserAsync(dbContext, "ana@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Beaches");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Santorini", "Greece");

            await AddReviewAsync(dbContext, user1.Id, destination.Id, 5);
            await AddReviewAsync(dbContext, user2.Id, destination.Id, 4);
            await AddReviewAsync(dbContext, user3.Id, destination.Id, 1, isHidden: true);

            var card = Assert.Single((await service.GetAllAsync(null, null, 1, DestinationSorting.NameAsc)).Destinations);

            Assert.Equal(2, card.ReviewsCount);
            Assert.Equal(4.5, card.AverageRating);
            Assert.Equal("Beaches", card.CategoryName);
            Assert.Equal("Greece", card.Country);
        }

        [Fact]
        public async Task GetAllAsync_CardWithoutReviews_HasNoRating()
        {
            var category = await AddCategoryAsync(dbContext, "Beaches");
            await AddDestinationAsync(dbContext, category.Id, "Sozopol");

            var card = Assert.Single((await service.GetAllAsync(null, null, 1, DestinationSorting.NameAsc)).Destinations);

            Assert.Equal(0, card.ReviewsCount);
            Assert.Null(card.AverageRating);
        }

        [Fact]
        public async Task GetDetailsAsync_ExistingDestination_ReturnsDetails()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");

            var result = await service.GetDetailsAsync(destination.Id);

            Assert.NotNull(result);
            Assert.Equal("Rome", result.Name);
            Assert.Equal("Italy", result.Country);
            Assert.Equal("Culture", result.CategoryName);
            Assert.Equal(category.Id, result.CategoryId);
            Assert.Equal(destination.Description, result.Description);
        }

        [Fact]
        public async Task GetDetailsAsync_MissingDestination_ReturnsNull()
        {
            var result = await service.GetDetailsAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetailsAsync_DeletedDestination_ReturnsNull()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);

            var result = await service.GetDetailsAsync(destination.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetailsAsync_SimilarDestinations_AreFromSameCategoryWithoutCurrentAndDeleted()
        {
            var mountains = await AddCategoryAsync(dbContext, "Mountains");
            var beaches = await AddCategoryAsync(dbContext, "Beaches");
            var bansko = await AddDestinationAsync(dbContext, mountains.Id, "Bansko");
            await AddDestinationAsync(dbContext, mountains.Id, "Zermatt", "Switzerland");
            await AddDestinationAsync(dbContext, mountains.Id, "Borovets");
            await AddDestinationAsync(dbContext, mountains.Id, "Interlaken", "Switzerland");
            await AddDestinationAsync(dbContext, mountains.Id, "Pamporovo");
            await AddDestinationAsync(dbContext, mountains.Id, "Aspen", "USA", isDeleted: true);
            await AddDestinationAsync(dbContext, beaches.Id, "Albena");

            var result = await service.GetDetailsAsync(bansko.Id);

            Assert.NotNull(result);
            Assert.Equal(new[] { "Borovets", "Interlaken", "Pamporovo" }, result.SimilarDestinations.Select(d => d.Name));
            Assert.Equal(PagingConstants.SimilarDestinationsCount, result.SimilarDestinations.Count());
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsCategoriesOrderedByName()
        {
            await AddCategoryAsync(dbContext, "Nature");
            await AddCategoryAsync(dbContext, "Culture");

            var result = await service.GetCategoriesAsync();

            Assert.Equal(new[] { "Culture", "Nature" }, result.Select(c => c.Name));
        }

        [Fact]
        public async Task AddAsync_NormalizesTextFields()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");

            await service.AddAsync(new DestinationFormViewModel
            {
                Name = "  Veliko   Tarnovo ",
                Country = " Bulgaria  ",
                Description = "  The old capital with the Tsarevets fortress.  ",
                ImageUrl = "  /images/destinations/tarnovo.jpg  ",
                CategoryId = category.Id
            });

            var destination = await dbContext.Destinations.SingleAsync();
            Assert.Equal("Veliko Tarnovo", destination.Name);
            Assert.Equal("Bulgaria", destination.Country);
            Assert.Equal("The old capital with the Tsarevets fortress.", destination.Description);
            Assert.Equal("/images/destinations/tarnovo.jpg", destination.ImageUrl);
            Assert.Equal(category.Id, destination.CategoryId);
            Assert.False(destination.IsDeleted);
        }

        [Fact]
        public async Task AddAsync_WithoutImage_SavesNullImageUrl()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");

            await service.AddAsync(new DestinationFormViewModel
            {
                Name = "Plovdiv",
                Country = "Bulgaria",
                Description = "Old town and Roman theatre.",
                CategoryId = category.Id
            });

            Assert.Null((await dbContext.Destinations.SingleAsync()).ImageUrl);
        }

        [Fact]
        public async Task GetForEditAsync_ExistingDestination_ReturnsFormModel()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");

            var result = await service.GetForEditAsync(destination.Id);

            Assert.NotNull(result);
            Assert.Equal("Rome", result.Name);
            Assert.Equal("Italy", result.Country);
            Assert.Equal(category.Id, result.CategoryId);
        }

        [Fact]
        public async Task GetForEditAsync_DeletedDestination_ReturnsNull()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);

            var result = await service.GetForEditAsync(destination.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task EditAsync_ExistingDestination_UpdatesAllFields()
        {
            var culture = await AddCategoryAsync(dbContext, "Culture");
            var beaches = await AddCategoryAsync(dbContext, "Beaches");
            var destination = await AddDestinationAsync(dbContext, culture.Id, "Varna");

            var result = await service.EditAsync(destination.Id, new DestinationFormViewModel
            {
                Name = " Golden   Sands ",
                Country = "Bulgaria",
                Description = " Long sandy beach near Varna. ",
                ImageUrl = null,
                CategoryId = beaches.Id
            });

            Assert.True(result);
            var updated = await dbContext.Destinations.SingleAsync();
            Assert.Equal("Golden Sands", updated.Name);
            Assert.Equal("Long sandy beach near Varna.", updated.Description);
            Assert.Null(updated.ImageUrl);
            Assert.Equal(beaches.Id, updated.CategoryId);
        }

        [Fact]
        public async Task EditAsync_MissingDestination_ReturnsFalse()
        {
            var result = await service.EditAsync(999, CreateForm(1));

            Assert.False(result);
        }

        [Fact]
        public async Task EditAsync_DeletedDestination_ReturnsFalseAndDoesNotChangeIt()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);

            var result = await service.EditAsync(destination.Id, CreateForm(category.Id));

            Assert.False(result);
            Assert.Equal("Old Town", (await dbContext.Destinations.IgnoreQueryFilters().SingleAsync()).Name);
        }

        [Fact]
        public async Task DeleteAsync_ExistingDestination_MarksItAsDeleted()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");

            var result = await service.DeleteAsync(destination.Id);

            Assert.True(result);
            Assert.Empty(dbContext.Destinations);
            Assert.True((await dbContext.Destinations.IgnoreQueryFilters().SingleAsync()).IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_MissingDestination_ReturnsFalse()
        {
            var result = await service.DeleteAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_AlreadyDeletedDestination_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);

            var result = await service.DeleteAsync(destination.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task CategoryExistsAsync_ExistingCategory_ReturnsTrue()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");

            Assert.True(await service.CategoryExistsAsync(category.Id));
        }

        [Fact]
        public async Task CategoryExistsAsync_MissingCategory_ReturnsFalse()
        {
            Assert.False(await service.CategoryExistsAsync(999));
        }

        [Fact]
        public async Task DestinationExistsAsync_SameNameAndCountryWithExtraSpaces_ReturnsTrue()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, category.Id, "Veliko Tarnovo");

            var result = await service.DestinationExistsAsync("  Veliko  Tarnovo ", " Bulgaria ");

            Assert.True(result);
        }

        [Fact]
        public async Task DestinationExistsAsync_SameNameInAnotherCountry_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, category.Id, "Alexandria", "Egypt");

            var result = await service.DestinationExistsAsync("Alexandria", "Romania");

            Assert.False(result);
        }

        [Fact]
        public async Task DestinationExistsAsync_ExcludedDestination_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Plovdiv");

            var result = await service.DestinationExistsAsync("Plovdiv", "Bulgaria", destination.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task DestinationExistsAsync_OnlyDeletedDestination_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);

            var result = await service.DestinationExistsAsync("Old Town", "Bulgaria");

            Assert.False(result);
        }

        [Fact]
        public async Task GetDetailsAsync_DestinationWithCoordinates_ReturnsThem()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Lisbon", "Portugal");
            destination.Latitude = 38.7223;
            destination.Longitude = -9.1393;
            await dbContext.SaveChangesAsync();

            var result = await service.GetDetailsAsync(destination.Id);

            Assert.NotNull(result);
            Assert.Equal(38.7223, result.Latitude);
            Assert.Equal(-9.1393, result.Longitude);
            Assert.True(result.HasCoordinates);
        }

        [Fact]
        public async Task GetDetailsAsync_DestinationWithoutCoordinates_HasNoCoordinates()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Plovdiv");

            var result = await service.GetDetailsAsync(destination.Id);

            Assert.NotNull(result);
            Assert.Null(result.Latitude);
            Assert.Null(result.Longitude);
            Assert.False(result.HasCoordinates);
        }

        [Fact]
        public async Task AddAsync_WithCoordinates_SavesThem()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var form = CreateForm(category.Id);
            form.Latitude = 42.1354;
            form.Longitude = 24.7453;

            await service.AddAsync(form);

            var destination = await dbContext.Destinations.SingleAsync();
            Assert.Equal(42.1354, destination.Latitude);
            Assert.Equal(24.7453, destination.Longitude);
        }

        [Fact]
        public async Task AddAsync_WithoutCoordinates_SavesNullCoordinates()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");

            await service.AddAsync(CreateForm(category.Id));

            var destination = await dbContext.Destinations.SingleAsync();
            Assert.Null(destination.Latitude);
            Assert.Null(destination.Longitude);
        }

        [Fact]
        public async Task GetForEditAsync_DestinationWithCoordinates_ReturnsThem()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Prague", "Czech Republic");
            destination.Latitude = 50.0755;
            destination.Longitude = 14.4378;
            await dbContext.SaveChangesAsync();

            var result = await service.GetForEditAsync(destination.Id);

            Assert.NotNull(result);
            Assert.Equal(50.0755, result.Latitude);
            Assert.Equal(14.4378, result.Longitude);
        }

        [Fact]
        public async Task EditAsync_NewCoordinates_UpdatesThem()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Varna");
            var form = CreateForm(category.Id);
            form.Latitude = 43.2141;
            form.Longitude = 27.9147;

            var result = await service.EditAsync(destination.Id, form);

            Assert.True(result);
            var updated = await dbContext.Destinations.SingleAsync();
            Assert.Equal(43.2141, updated.Latitude);
            Assert.Equal(27.9147, updated.Longitude);
        }

        [Fact]
        public async Task EditAsync_EmptyCoordinates_RemovesThem()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var destination = await AddDestinationAsync(dbContext, category.Id, "Varna");
            destination.Latitude = 43.2141;
            destination.Longitude = 27.9147;
            await dbContext.SaveChangesAsync();

            var result = await service.EditAsync(destination.Id, CreateForm(category.Id));

            Assert.True(result);
            var updated = await dbContext.Destinations.SingleAsync();
            Assert.Null(updated.Latitude);
            Assert.Null(updated.Longitude);
        }

        private static DestinationFormViewModel CreateForm(int categoryId)
        {
            return new DestinationFormViewModel
            {
                Name = "New name",
                Country = "Bulgaria",
                Description = "A new description for the destination.",
                CategoryId = categoryId
            };
        }
    }
}
