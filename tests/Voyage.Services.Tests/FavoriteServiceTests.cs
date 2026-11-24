using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Common.Constants.Validation;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Results;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Enums;
using Voyage.Web.ViewModels.Favorites;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class FavoriteServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly FavoriteService service;

        public FavoriteServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new FavoriteService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task AddAsync_NewFavorite_SavesItAndReturnsSuccess()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var santorini = await AddDestinationAsync("Santorini", "Greece");

            var result = await service.AddAsync(santorini.Id, user.Id);

            Assert.Equal(FavoriteResult.Success, result);
            var favorite = await dbContext.Favorites.SingleAsync();
            Assert.Equal(user.Id, favorite.UserId);
            Assert.Equal(santorini.Id, favorite.DestinationId);
            Assert.True(favorite.AddedOn > DateTime.UtcNow.AddMinutes(-1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(999)]
        public async Task AddAsync_InvalidOrMissingDestination_ReturnsDestinationNotFound(int destinationId)
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");

            var result = await service.AddAsync(destinationId, user.Id);

            Assert.Equal(FavoriteResult.DestinationNotFound, result);
            Assert.Empty(dbContext.Favorites);
        }

        [Fact]
        public async Task AddAsync_DeletedDestination_ReturnsDestinationNotFound()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var oldTown = await AddDestinationAsync("Old Town", "Bulgaria", isDeleted: true);

            Assert.Equal(FavoriteResult.DestinationNotFound, await service.AddAsync(oldTown.Id, user.Id));
        }

        [Fact]
        public async Task AddAsync_DestinationAlreadyInFavorites_ReturnsAlreadyExists()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var santorini = await AddDestinationAsync("Santorini", "Greece");
            await AddFavoriteAsync(user, santorini, daysAgo: 3);

            var result = await service.AddAsync(santorini.Id, user.Id);

            Assert.Equal(FavoriteResult.AlreadyExists, result);
            Assert.Single(dbContext.Favorites);
        }

        [Fact]
        public async Task AddAsync_UserReachedTheLimit_ReturnsLimitReached()
        {
            var user = await AddUserAsync(dbContext, "collector@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Everywhere");

            for (var i = 0; i < FavoriteValidationConstants.MaxPerUser; i++)
            {
                var destination = new Destination
                {
                    Name = $"Place {i}",
                    Country = "Bulgaria",
                    Description = "A place used in tests.",
                    CategoryId = category.Id
                };

                dbContext.Destinations.Add(destination);
                dbContext.Favorites.Add(new Favorite { UserId = user.Id, Destination = destination });
            }

            var extra = new Destination
            {
                Name = "One more place",
                Country = "Bulgaria",
                Description = "A place used in tests.",
                CategoryId = category.Id
            };
            dbContext.Destinations.Add(extra);
            await dbContext.SaveChangesAsync();

            var result = await service.AddAsync(extra.Id, user.Id);

            Assert.Equal(FavoriteResult.LimitReached, result);
            Assert.Equal(FavoriteValidationConstants.MaxPerUser, await dbContext.Favorites.CountAsync());
        }

        [Fact]
        public async Task AddAsync_SameDestinationForDifferentUsers_IsAllowed()
        {
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");
            await AddFavoriteAsync(maria, rome);

            Assert.Equal(FavoriteResult.Success, await service.AddAsync(rome.Id, georgi.Id));
            Assert.Equal(2, await dbContext.Favorites.CountAsync());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AddAsync_MissingUser_Throws(string? userId)
        {
            var rome = await AddDestinationAsync("Rome", "Italy");

            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.AddAsync(rome.Id, userId!));
        }

        [Fact]
        public async Task RemoveAsync_ExistingFavorite_RemovesItAndReturnsSuccess()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");
            var paris = await AddDestinationAsync("Paris", "France");
            await AddFavoriteAsync(user, rome);
            await AddFavoriteAsync(user, paris);

            var result = await service.RemoveAsync(rome.Id, user.Id);

            Assert.Equal(FavoriteResult.Success, result);
            Assert.Equal(paris.Id, (await dbContext.Favorites.SingleAsync()).DestinationId);
        }

        [Fact]
        public async Task RemoveAsync_DestinationNotInFavorites_ReturnsNotInFavorites()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");

            Assert.Equal(FavoriteResult.NotInFavorites, await service.RemoveAsync(rome.Id, user.Id));
        }

        [Fact]
        public async Task RemoveAsync_FavoriteOfAnotherUser_IsNotRemoved()
        {
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");
            await AddFavoriteAsync(maria, rome);

            var result = await service.RemoveAsync(rome.Id, georgi.Id);

            Assert.Equal(FavoriteResult.NotInFavorites, result);
            Assert.Single(dbContext.Favorites);
        }

        [Fact]
        public async Task RemoveAsync_MissingUser_Throws()
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.RemoveAsync(1, " "));
        }

        [Fact]
        public async Task IsFavoriteAsync_SavedDestination_ReturnsTrue()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");
            await AddFavoriteAsync(user, rome);

            Assert.True(await service.IsFavoriteAsync(rome.Id, user.Id));
        }

        [Fact]
        public async Task IsFavoriteAsync_NotSavedDestination_ReturnsFalse()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");

            Assert.False(await service.IsFavoriteAsync(rome.Id, user.Id));
        }

        [Fact]
        public async Task GetFavoriteDestinationIdsAsync_ReturnsOnlyTheSavedOnesFromTheList()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");
            var paris = await AddDestinationAsync("Paris", "France");
            var prague = await AddDestinationAsync("Prague", "Czech Republic");
            await AddFavoriteAsync(user, rome);
            await AddFavoriteAsync(user, prague);

            var result = await service.GetFavoriteDestinationIdsAsync(
                user.Id, new[] { rome.Id, paris.Id, rome.Id });

            Assert.Equal(new HashSet<int> { rome.Id }, result);
        }

        [Fact]
        public async Task GetFavoriteDestinationIdsAsync_EmptyList_ReturnsEmptySet()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");

            Assert.Empty(await service.GetFavoriteDestinationIdsAsync(user.Id, Array.Empty<int>()));
        }

        [Fact]
        public async Task GetFavoriteDestinationIdsAsync_DeletedDestination_IsNotReturned()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var oldTown = await AddDestinationAsync("Old Town", "Bulgaria");
            await AddFavoriteAsync(user, oldTown);
            oldTown.IsDeleted = true;
            await dbContext.SaveChangesAsync();

            Assert.Empty(await service.GetFavoriteDestinationIdsAsync(user.Id, new[] { oldTown.Id }));
        }

        [Fact]
        public async Task GetFavoriteDestinationIdsAsync_MissingUser_Throws()
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(
                () => service.GetFavoriteDestinationIdsAsync("", new[] { 1 }));
        }

        [Fact]
        public async Task GetCountForDestinationAsync_CountsAllUsersWhoSavedIt()
        {
            var rome = await AddDestinationAsync("Rome", "Italy");
            var paris = await AddDestinationAsync("Paris", "France");
            var users = await AddUsersAsync(3);
            await AddFavoriteAsync(users[0], rome);
            await AddFavoriteAsync(users[1], rome);
            await AddFavoriteAsync(users[2], paris);

            Assert.Equal(2, await service.GetCountForDestinationAsync(rome.Id));
            Assert.Equal(1, await service.GetCountForDestinationAsync(paris.Id));
        }

        [Fact]
        public async Task GetUserFavoritesCountAsync_CountsOnlyOwnActiveFavorites()
        {
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var rome = await AddDestinationAsync("Rome", "Italy");
            var paris = await AddDestinationAsync("Paris", "France");
            var oldTown = await AddDestinationAsync("Old Town", "Bulgaria");
            await AddFavoriteAsync(maria, rome);
            await AddFavoriteAsync(maria, oldTown);
            await AddFavoriteAsync(georgi, paris);
            oldTown.IsDeleted = true;
            await dbContext.SaveChangesAsync();

            Assert.Equal(1, await service.GetUserFavoritesCountAsync(maria.Id));
        }

        [Fact]
        public async Task GetMineAsync_DefaultSorting_ShowsRecentlyAddedFirst()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddFavoritesAsync(user, ("Prague", 30), ("Santorini", 2), ("Budapest", 10));

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel());

            Assert.Equal(FavoritesSorting.Recent, result.Sorting);
            Assert.Equal(new[] { "Santorini", "Budapest", "Prague" }, Names(result));
        }

        [Fact]
        public async Task GetMineAsync_Oldest_ShowsFirstAddedFirst()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddFavoritesAsync(user, ("Prague", 30), ("Santorini", 2), ("Budapest", 10));

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel { Sorting = FavoritesSorting.Oldest });

            Assert.Equal(new[] { "Prague", "Budapest", "Santorini" }, Names(result));
        }

        [Fact]
        public async Task GetMineAsync_Name_SortsAlphabetically()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddFavoritesAsync(user, ("Prague", 30), ("Santorini", 2), ("Budapest", 10));

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel { Sorting = FavoritesSorting.Name });

            Assert.Equal(new[] { "Budapest", "Prague", "Santorini" }, Names(result));
        }

        [Fact]
        public async Task GetMineAsync_Rating_SortsByAverageWithoutHiddenReviews()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var reviewer = await AddUserAsync(dbContext, "georgi@voyage.test");
            var favorites = await AddFavoritesAsync(user, ("Prague", 1), ("Santorini", 2), ("Budapest", 3), ("Akureyri", 4));
            await TestData.AddReviewAsync(dbContext, reviewer.Id, favorites["Prague"].Id, 4);
            await TestData.AddReviewAsync(dbContext, reviewer.Id, favorites["Santorini"].Id, 5);
            await TestData.AddReviewAsync(dbContext, reviewer.Id, favorites["Budapest"].Id, 3);
            await TestData.AddReviewAsync(dbContext, user.Id, favorites["Budapest"].Id, 5, isHidden: true);

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel { Sorting = FavoritesSorting.Rating });

            Assert.Equal(new[] { "Santorini", "Prague", "Budapest", "Akureyri" }, Names(result));
        }

        [Fact]
        public async Task GetMineAsync_UnknownSortingValue_FallsBackToRecent()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddFavoritesAsync(user, ("Prague", 30), ("Santorini", 2));

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel { Sorting = (FavoritesSorting)99 });

            Assert.Equal(FavoritesSorting.Recent, result.Sorting);
            Assert.Equal(new[] { "Santorini", "Prague" }, Names(result));
        }

        [Fact]
        public async Task GetMineAsync_SearchByName_ReturnsMatchesAndKeepsTheTotalSavedCount()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddFavoritesAsync(user, ("Prague", 30), ("Santorini", 2), ("Budapest", 10));

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel { Search = "  Prag " });

            Assert.Equal(new[] { "Prague" }, Names(result));
            Assert.Equal("Prag", result.Search);
            Assert.Equal(1, result.ResultsCount);
            Assert.Equal(3, result.TotalSaved);
        }

        [Fact]
        public async Task GetMineAsync_SearchByCountry_ReturnsMatches()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var category = await AddCategoryAsync(dbContext, "Mountains");
            var zermatt = await TestData.AddDestinationAsync(dbContext, category.Id, "Zermatt", "Switzerland");
            var bansko = await TestData.AddDestinationAsync(dbContext, category.Id, "Bansko", "Bulgaria");
            await AddFavoriteAsync(user, zermatt);
            await AddFavoriteAsync(user, bansko);

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel { Search = "Switzerland" });

            Assert.Equal(new[] { "Zermatt" }, Names(result));
        }

        [Fact]
        public async Task GetMineAsync_SecondPage_ReturnsTheRemainingFavorites()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var items = Enumerable.Range(1, PagingConstants.FavoritesPerPage + 2)
                .Select(i => ($"City {i:D2}", i))
                .ToArray();
            await AddFavoritesAsync(user, items);

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel
            {
                Sorting = FavoritesSorting.Name,
                Page = 2
            });

            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(PagingConstants.FavoritesPerPage + 2, result.ResultsCount);
            Assert.Equal(new[] { "City 10", "City 11" }, Names(result));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(100, 2)]
        public async Task GetMineAsync_PageOutOfRange_IsClamped(int page, int expectedPage)
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            var items = Enumerable.Range(1, PagingConstants.FavoritesPerPage + 1)
                .Select(i => ($"City {i:D2}", i))
                .ToArray();
            await AddFavoritesAsync(user, items);

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel { Page = page });

            Assert.Equal(expectedPage, result.CurrentPage);
        }

        [Fact]
        public async Task GetMineAsync_NoFavorites_ReturnsOneEmptyPage()
        {
            var user = await AddUserAsync(dbContext, "new@voyage.test");

            var result = await service.GetMineAsync(user.Id, new FavoritesQueryModel());

            Assert.Empty(result.Destinations);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(0, result.TotalSaved);
        }

        [Fact]
        public async Task GetMineAsync_NullQuery_UsesTheDefaults()
        {
            var user = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddFavoritesAsync(user, ("Prague", 3));

            var result = await service.GetMineAsync(user.Id, null!);

            Assert.Equal(FavoritesSorting.Recent, result.Sorting);
            Assert.Equal(1, result.CurrentPage);
            Assert.Single(result.Destinations);
        }

        [Fact]
        public async Task GetMineAsync_ReturnsOnlyOwnActiveFavoritesMarkedAsFavorite()
        {
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var mine = await AddFavoritesAsync(maria, ("Prague", 3), ("Old Town", 5));
            await AddFavoritesAsync(georgi, ("Kyoto", 1));
            mine["Old Town"].IsDeleted = true;
            await dbContext.SaveChangesAsync();

            var result = await service.GetMineAsync(maria.Id, new FavoritesQueryModel());

            var card = Assert.Single(result.Destinations);
            Assert.Equal("Prague", card.Name);
            Assert.True(card.IsFavorite);
            Assert.Equal(FavoriteValidationConstants.MaxPerUser, result.MaxFavorites);
        }

        [Fact]
        public async Task GetMineAsync_MissingUser_Throws()
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(
                () => service.GetMineAsync(" ", new FavoritesQueryModel()));
        }

        private async Task<Destination> AddDestinationAsync(string name, string country, bool isDeleted = false)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync()
                ?? await AddCategoryAsync(dbContext, "Culture");

            return await TestData.AddDestinationAsync(dbContext, category.Id, name, country, isDeleted);
        }

        private async Task AddFavoriteAsync(IdentityUser user, Destination destination, int daysAgo = 0)
        {
            dbContext.Favorites.Add(new Favorite
            {
                UserId = user.Id,
                DestinationId = destination.Id,
                AddedOn = DateTime.UtcNow.AddDays(-daysAgo)
            });

            await dbContext.SaveChangesAsync();
        }

        private async Task<Dictionary<string, Destination>> AddFavoritesAsync(
            IdentityUser user,
            params (string Name, int DaysAgo)[] items)
        {
            var destinations = new Dictionary<string, Destination>();

            foreach (var (name, daysAgo) in items)
            {
                var destination = await dbContext.Destinations.FirstOrDefaultAsync(d => d.Name == name)
                    ?? await AddDestinationAsync(name, "Europe");

                await AddFavoriteAsync(user, destination, daysAgo);
                destinations[name] = destination;
            }

            return destinations;
        }

        private async Task<List<IdentityUser>> AddUsersAsync(int count)
        {
            var users = new List<IdentityUser>();

            for (var i = 0; i < count; i++)
            {
                users.Add(await AddUserAsync(dbContext, $"user{i}@voyage.test"));
            }

            return users;
        }

        private static string[] Names(FavoritesPageViewModel page)
        {
            return page.Destinations.Select(d => d.Name).ToArray();
        }
    }
}
