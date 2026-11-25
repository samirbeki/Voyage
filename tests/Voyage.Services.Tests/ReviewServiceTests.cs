using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.Reviews;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class ReviewServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ReviewService service;

        public ReviewServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new ReviewService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetForDestinationAsync_ReturnsReviewsOfTheDestinationNewestFirst()
        {
            var (rome, paris) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            await AddReviewAsync(maria, rome, 4, daysAgo: 10);
            await AddReviewAsync(georgi, rome, 5, daysAgo: 2);
            await AddReviewAsync(maria, paris, 3, daysAgo: 1);

            var result = (await service.GetForDestinationAsync(rome.Id, null, false)).ToList();

            Assert.Equal(new[] { 5, 4 }, result.Select(r => r.Rating));
        }

        [Fact]
        public async Task GetForDestinationAsync_HiddenReviewsOfOthers_AreNotShown()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            await AddReviewAsync(maria, rome, 4);
            await AddReviewAsync(georgi, rome, 1, isHidden: true);

            var result = await service.GetForDestinationAsync(rome.Id, maria.Id, false);

            Assert.Equal(4, Assert.Single(result).Rating);
        }

        [Fact]
        public async Task GetForDestinationAsync_OwnHiddenReview_IsShownToItsAuthor()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            await AddReviewAsync(georgi, rome, 1, isHidden: true);

            var review = Assert.Single(await service.GetForDestinationAsync(rome.Id, georgi.Id, false));

            Assert.True(review.IsMine);
            Assert.True(review.IsHidden);
        }

        [Fact]
        public async Task GetForDestinationAsync_IncludeHidden_ShowsAllReviews()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            await AddReviewAsync(maria, rome, 4);
            await AddReviewAsync(georgi, rome, 1, isHidden: true);

            var result = await service.GetForDestinationAsync(rome.Id, null, true);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetForDestinationAsync_DeletedReviews_AreNotShown()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddReviewAsync(maria, rome, 4, isDeleted: true);

            Assert.Empty(await service.GetForDestinationAsync(rome.Id, maria.Id, true));
        }

        [Fact]
        public async Task GetForDestinationAsync_EmailAuthorName_ShowsOnlyThePartBeforeTheAt()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddReviewAsync(maria, rome, 5);

            var review = Assert.Single(await service.GetForDestinationAsync(rome.Id, null, false));

            Assert.Equal("maria", review.AuthorName);
            Assert.False(review.IsMine);
        }

        [Fact]
        public async Task GetForDestinationAsync_UserNameWithoutAt_IsShownAsItIs()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var user = new IdentityUser { UserName = "traveler_bg" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            await AddReviewAsync(user, rome, 5);

            var review = Assert.Single(await service.GetForDestinationAsync(rome.Id, null, false));

            Assert.Equal("traveler_bg", review.AuthorName);
        }

        [Fact]
        public async Task GetForDestinationAsync_ModifiedReview_IsMarkedAsEdited()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var review = await AddReviewAsync(maria, rome, 5);
            review.ModifiedOn = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            var result = Assert.Single(await service.GetForDestinationAsync(rome.Id, maria.Id, false));

            Assert.True(result.IsEdited);
            Assert.True(result.IsMine);
        }

        [Fact]
        public async Task GetRatingSummaryAsync_ReturnsRoundedAverageWithoutHiddenReviews()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var users = await AddUsersAsync(4);
            await AddReviewAsync(users[0], rome, 5);
            await AddReviewAsync(users[1], rome, 5);
            await AddReviewAsync(users[2], rome, 4);
            await AddReviewAsync(users[3], rome, 1, isHidden: true);

            var result = await service.GetRatingSummaryAsync(rome.Id);

            Assert.Equal(3, result.Count);
            Assert.Equal(4.7, result.Average);
        }

        [Fact]
        public async Task GetRatingSummaryAsync_NoReviews_ReturnsZero()
        {
            var (rome, _) = await AddTwoDestinationsAsync();

            var result = await service.GetRatingSummaryAsync(rome.Id);

            Assert.Equal(0, result.Count);
            Assert.Equal(0, result.Average);
        }

        [Fact]
        public async Task GetRatingSummaryAsync_DeletedReviews_AreNotCounted()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var users = await AddUsersAsync(2);
            await AddReviewAsync(users[0], rome, 4);
            await AddReviewAsync(users[1], rome, 1, isDeleted: true);

            var result = await service.GetRatingSummaryAsync(rome.Id);

            Assert.Equal(1, result.Count);
            Assert.Equal(4, result.Average);
        }

        [Fact]
        public async Task DestinationExistsAsync_ActiveDestination_ReturnsTrue()
        {
            var (rome, _) = await AddTwoDestinationsAsync();

            Assert.True(await service.DestinationExistsAsync(rome.Id));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        [InlineData(999)]
        public async Task DestinationExistsAsync_InvalidOrMissingId_ReturnsFalse(int id)
        {
            await AddTwoDestinationsAsync();

            Assert.False(await service.DestinationExistsAsync(id));
        }

        [Fact]
        public async Task DestinationExistsAsync_DeletedDestination_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var oldTown = await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);

            Assert.False(await service.DestinationExistsAsync(oldTown.Id));
        }

        [Fact]
        public async Task HasUserReviewedAsync_ExistingReview_ReturnsTrue()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddReviewAsync(maria, rome, 4);

            Assert.True(await service.HasUserReviewedAsync(rome.Id, maria.Id));
        }

        [Fact]
        public async Task HasUserReviewedAsync_ReviewOfAnotherDestination_ReturnsFalse()
        {
            var (rome, paris) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddReviewAsync(maria, paris, 4);

            Assert.False(await service.HasUserReviewedAsync(rome.Id, maria.Id));
        }

        [Fact]
        public async Task HasUserReviewedAsync_DeletedReview_ReturnsFalse()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddReviewAsync(maria, rome, 4, isDeleted: true);

            Assert.False(await service.HasUserReviewedAsync(rome.Id, maria.Id));
        }

        [Fact]
        public async Task CreateAsync_NewReview_SavesTrimmedComment()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");

            var result = await service.CreateAsync(rome.Id, new ReviewFormModel
            {
                Rating = 5,
                Comment = "  Every street feels like a museum.  "
            }, maria.Id);

            Assert.True(result);
            var review = await dbContext.Reviews.SingleAsync();
            Assert.Equal(5, review.Rating);
            Assert.Equal("Every street feels like a museum.", review.Comment);
            Assert.Equal(maria.Id, review.AuthorId);
            Assert.Equal(rome.Id, review.DestinationId);
            Assert.False(review.IsHidden);
            Assert.True(review.CreatedOn > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task CreateAsync_MissingDestination_ReturnsFalse()
        {
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");

            Assert.False(await service.CreateAsync(999, CreateForm(), maria.Id));
            Assert.Empty(dbContext.Reviews);
        }

        [Fact]
        public async Task CreateAsync_DeletedDestination_ReturnsFalse()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var oldTown = await AddDestinationAsync(dbContext, category.Id, "Old Town", isDeleted: true);
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");

            Assert.False(await service.CreateAsync(oldTown.Id, CreateForm(), maria.Id));
        }

        [Fact]
        public async Task CreateAsync_SecondReviewForTheSameDestination_ReturnsFalse()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddReviewAsync(maria, rome, 4);

            var result = await service.CreateAsync(rome.Id, CreateForm(), maria.Id);

            Assert.False(result);
            Assert.Single(dbContext.Reviews);
        }

        [Fact]
        public async Task GetForEditAsync_OwnReview_ReturnsItWithTheDestinationName()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var review = await AddReviewAsync(maria, rome, 4);

            var result = await service.GetForEditAsync(review.Id, maria.Id);

            Assert.NotNull(result);
            Assert.Equal(rome.Id, result.DestinationId);
            Assert.Equal("Rome", result.DestinationName);
            Assert.Equal(4, result.Rating);
            Assert.Equal(review.Comment, result.Comment);
        }

        [Fact]
        public async Task GetForEditAsync_ReviewOfAnotherUser_ReturnsNull()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var review = await AddReviewAsync(maria, rome, 4);

            Assert.Null(await service.GetForEditAsync(review.Id, georgi.Id));
        }

        [Fact]
        public async Task EditAsync_OwnReview_UpdatesItAndReturnsTheDestinationId()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var review = await AddReviewAsync(maria, rome, 3);

            var result = await service.EditAsync(review.Id, new ReviewFormModel
            {
                Rating = 5,
                Comment = "  Changed my mind after the second visit.  "
            }, maria.Id);

            Assert.Equal(rome.Id, result);
            var updated = await dbContext.Reviews.SingleAsync();
            Assert.Equal(5, updated.Rating);
            Assert.Equal("Changed my mind after the second visit.", updated.Comment);
            Assert.NotNull(updated.ModifiedOn);
        }

        [Fact]
        public async Task EditAsync_ReviewOfAnotherUser_ReturnsNullAndKeepsIt()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var review = await AddReviewAsync(maria, rome, 4);

            var result = await service.EditAsync(review.Id, CreateForm(), georgi.Id);

            Assert.Null(result);
            var unchanged = await dbContext.Reviews.SingleAsync();
            Assert.Equal(4, unchanged.Rating);
            Assert.Null(unchanged.ModifiedOn);
        }

        [Fact]
        public async Task DeleteAsync_OwnReview_MarksItAsDeletedAndReturnsTheDestinationId()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var review = await AddReviewAsync(maria, rome, 4);

            var result = await service.DeleteAsync(review.Id, maria.Id);

            Assert.Equal(rome.Id, result);
            Assert.Empty(dbContext.Reviews);
            Assert.True((await dbContext.Reviews.IgnoreQueryFilters().SingleAsync()).IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ReviewOfAnotherUser_ReturnsNullAndKeepsIt()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var review = await AddReviewAsync(maria, rome, 4);

            Assert.Null(await service.DeleteAsync(review.Id, georgi.Id));
            Assert.Single(dbContext.Reviews);
        }

        [Fact]
        public async Task GetForModerationAsync_AllStatus_ReturnsVisibleAndHiddenNewestFirst()
        {
            var (rome, paris) = await AddTwoDestinationsAsync();
            var users = await AddUsersAsync(3);
            await AddReviewAsync(users[0], rome, 5, daysAgo: 5);
            await AddReviewAsync(users[1], paris, 1, isHidden: true, daysAgo: 1);
            await AddReviewAsync(users[2], rome, 3, daysAgo: 3);

            var result = await service.GetForModerationAsync("all", 1);

            Assert.Equal("all", result.Status);
            Assert.Equal(new[] { 1, 3, 5 }, result.Reviews.Select(r => r.Rating));
            var first = result.Reviews.First();
            Assert.Equal("Paris", first.DestinationName);
            Assert.Equal("user1@voyage.test", first.AuthorName);
            Assert.True(first.IsHidden);
        }

        [Theory]
        [InlineData("visible", 2)]
        [InlineData("hidden", 1)]
        [InlineData("HIDDEN", 1)]
        [InlineData("unknown", 3)]
        [InlineData(null, 3)]
        public async Task GetForModerationAsync_FiltersByStatus(string? status, int expectedCount)
        {
            var (rome, paris) = await AddTwoDestinationsAsync();
            var users = await AddUsersAsync(3);
            await AddReviewAsync(users[0], rome, 5);
            await AddReviewAsync(users[1], paris, 1, isHidden: true);
            await AddReviewAsync(users[2], rome, 3);

            var result = await service.GetForModerationAsync(status!, 1);

            Assert.Equal(expectedCount, result.Reviews.Count());
        }

        [Fact]
        public async Task GetForModerationAsync_SecondPage_ReturnsTheOldestReviews()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var users = await AddUsersAsync(PagingConstants.ModerationReviewsPerPage + 1);
            for (var i = 0; i < users.Count; i++)
            {
                await AddReviewAsync(users[i], rome, 4, daysAgo: i);
            }

            var result = await service.GetForModerationAsync("all", 2);

            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(
                $"user{PagingConstants.ModerationReviewsPerPage}@voyage.test",
                Assert.Single(result.Reviews).AuthorName);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-4, 1)]
        [InlineData(50, 2)]
        public async Task GetForModerationAsync_PageOutOfRange_IsClamped(int requestedPage, int expectedPage)
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var users = await AddUsersAsync(PagingConstants.ModerationReviewsPerPage + 1);
            foreach (var user in users)
            {
                await AddReviewAsync(user, rome, 4);
            }

            var result = await service.GetForModerationAsync("all", requestedPage);

            Assert.Equal(expectedPage, result.CurrentPage);
        }

        [Fact]
        public async Task GetForModerationAsync_NoReviews_ReturnsOneEmptyPage()
        {
            var result = await service.GetForModerationAsync("all", 3);

            Assert.Empty(result.Reviews);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(1, result.TotalPages);
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task SetHiddenAsync_ChangesTheVisibility(bool initiallyHidden, bool newValue)
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var review = await AddReviewAsync(maria, rome, 4, isHidden: initiallyHidden);

            var result = await service.SetHiddenAsync(review.Id, newValue);

            Assert.True(result);
            Assert.Equal(newValue, (await dbContext.Reviews.SingleAsync()).IsHidden);
        }

        [Fact]
        public async Task SetHiddenAsync_MissingReview_ReturnsFalse()
        {
            Assert.False(await service.SetHiddenAsync(999, true));
        }

        [Fact]
        public async Task DeleteByModeratorAsync_ReviewOfAnyUser_MarksItAsDeleted()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var review = await AddReviewAsync(maria, rome, 1);

            var result = await service.DeleteByModeratorAsync(review.Id);

            Assert.True(result);
            Assert.Empty(dbContext.Reviews);
        }

        [Fact]
        public async Task DeleteByModeratorAsync_MissingReview_ReturnsFalse()
        {
            Assert.False(await service.DeleteByModeratorAsync(999));
        }

        [Fact]
        public async Task GetForDestinationAsync_ExpertReviews_AreShownFirstAndMarked()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var ivan = await AddUserAsync(dbContext, "ivan@voyage.test");
            await MakeExpertAsync(georgi);
            await AddReviewAsync(maria, rome, 5, daysAgo: 1);
            await AddReviewAsync(georgi, rome, 4, daysAgo: 30);
            await AddReviewAsync(ivan, rome, 3, daysAgo: 5);

            var result = (await service.GetForDestinationAsync(rome.Id, null, false)).ToList();

            Assert.Equal(new[] { "georgi", "maria", "ivan" }, result.Select(r => r.AuthorName));
            Assert.Equal(new[] { true, false, false }, result.Select(r => r.IsExpert));
        }

        [Fact]
        public async Task GetForDestinationAsync_SeveralExperts_AreOrderedNewestFirst()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            var georgi = await AddUserAsync(dbContext, "georgi@voyage.test");
            var ivan = await AddUserAsync(dbContext, "ivan@voyage.test");
            await MakeExpertAsync(maria);
            await MakeExpertAsync(georgi);
            await AddReviewAsync(maria, rome, 5, daysAgo: 20);
            await AddReviewAsync(georgi, rome, 4, daysAgo: 3);
            await AddReviewAsync(ivan, rome, 2, daysAgo: 1);

            var result = (await service.GetForDestinationAsync(rome.Id, null, false)).ToList();

            Assert.Equal(new[] { "georgi", "maria", "ivan" }, result.Select(r => r.AuthorName));
        }

        [Fact]
        public async Task GetForDestinationAsync_WithoutExpertRole_NoReviewIsMarked()
        {
            var (rome, _) = await AddTwoDestinationsAsync();
            var maria = await AddUserAsync(dbContext, "maria@voyage.test");
            await AddReviewAsync(maria, rome, 5);

            var result = await service.GetForDestinationAsync(rome.Id, null, false);

            Assert.False(Assert.Single(result).IsExpert);
        }

        private async Task MakeExpertAsync(IdentityUser user)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.TravelExpert);

            if (role == null)
            {
                role = new IdentityRole(RoleConstants.TravelExpert);
                dbContext.Roles.Add(role);
            }

            dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
            await dbContext.SaveChangesAsync();
        }

        private async Task<(Destination Rome, Destination Paris)> AddTwoDestinationsAsync()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");

            return (rome, paris);
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

        private async Task<Review> AddReviewAsync(
            IdentityUser author,
            Destination destination,
            int rating,
            bool isHidden = false,
            bool isDeleted = false,
            int daysAgo = 0)
        {
            var review = await TestData.AddReviewAsync(
                dbContext, author.Id, destination.Id, rating, isHidden, isDeleted);

            review.CreatedOn = DateTime.UtcNow.AddDays(-daysAgo);
            await dbContext.SaveChangesAsync();

            return review;
        }

        private static ReviewFormModel CreateForm()
        {
            return new ReviewFormModel
            {
                Rating = 2,
                Comment = "Somebody else's opinion."
            };
        }
    }
}
