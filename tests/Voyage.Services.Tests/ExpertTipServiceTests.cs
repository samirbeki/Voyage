using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Services.Results;
using Voyage.Services.Tests.Infrastructure;
using Voyage.Web.ViewModels.ExpertTips;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class ExpertTipServiceTests : IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ExpertTipService service;

        public ExpertTipServiceTests()
        {
            dbContext = TestDbContextFactory.Create();
            service = new ExpertTipService(dbContext);
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task GetForDestinationAsync_ReturnsTipsOfExpertsNewestFirst()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var georgi = await AddExpertAsync("georgi@voyage.test");
            await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.", daysAgo: 30);
            await AddTipAsync(georgi, rome, "Trastevere is the best area for dinner, away from the crowds.", daysAgo: 2);

            var result = (await service.GetForDestinationAsync(rome.Id, null)).ToList();

            Assert.Equal(new[] { "georgi", "maria" }, result.Select(t => t.AuthorName));
            Assert.Equal("Trastevere is the best area for dinner, away from the crowds.", result[0].Content);
            Assert.All(result, t => Assert.False(t.IsMine));
        }

        [Fact]
        public async Task GetForDestinationAsync_AuthorIsNoLongerExpert_TipIsHidden()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var ivan = await AddExpertAsync("ivan@voyage.test");
            await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");
            await AddTipAsync(ivan, rome, "Avoid the restaurants right next to the Trevi Fountain.");
            await RemoveExpertAsync(ivan);

            var result = await service.GetForDestinationAsync(rome.Id, null);

            Assert.Equal("maria", Assert.Single(result).AuthorName);
        }

        [Fact]
        public async Task GetForDestinationAsync_ReturnsOnlyTipsOfTheGivenDestination()
        {
            var (rome, paris) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");
            await AddTipAsync(maria, paris, "Climb the Arc de Triomphe instead of the Eiffel Tower.");

            var result = await service.GetForDestinationAsync(paris.Id, null);

            Assert.Equal("Climb the Arc de Triomphe instead of the Eiffel Tower.", Assert.Single(result).Content);
        }

        [Fact]
        public async Task GetForDestinationAsync_MarksOwnAndEditedTips()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var georgi = await AddExpertAsync("georgi@voyage.test");
            var own = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.", daysAgo: 5);
            await AddTipAsync(georgi, rome, "Trastevere is the best area for dinner, away from the crowds.", daysAgo: 1);
            own.ModifiedOn = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            var result = (await service.GetForDestinationAsync(rome.Id, maria.Id)).ToList();

            var mine = result.Single(t => t.Id == own.Id);
            Assert.True(mine.IsMine);
            Assert.True(mine.IsEdited);
            Assert.False(result.Single(t => t.Id != own.Id).IsMine);
        }

        [Fact]
        public async Task GetForDestinationAsync_NoExpertRole_ReturnsNothing()
        {
            var (rome, _) = await AddDestinationsAsync();
            var ivan = await AddUserAsync(dbContext, "ivan@voyage.test");
            await AddTipAsync(ivan, rome, "Avoid the restaurants right next to the Trevi Fountain.");

            Assert.Empty(await service.GetForDestinationAsync(rome.Id, null));
        }

        [Fact]
        public async Task GetForDestinationAsync_DeletedDestination_ReturnsNothing()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");
            rome.IsDeleted = true;
            await dbContext.SaveChangesAsync();

            Assert.Empty(await service.GetForDestinationAsync(rome.Id, null));
        }

        [Fact]
        public async Task CanAddAsync_ExpertWithoutTip_ReturnsTrue()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");

            Assert.True(await service.CanAddAsync(rome.Id, maria.Id));
        }

        [Fact]
        public async Task CanAddAsync_ExpertAlreadyHasTip_ReturnsFalse()
        {
            var (rome, paris) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            Assert.False(await service.CanAddAsync(rome.Id, maria.Id));
            Assert.True(await service.CanAddAsync(paris.Id, maria.Id));
        }

        [Fact]
        public async Task CanAddAsync_RegularUser_ReturnsFalse()
        {
            var (rome, _) = await AddDestinationsAsync();
            await AddExpertAsync("maria@voyage.test");
            var ivan = await AddUserAsync(dbContext, "ivan@voyage.test");

            Assert.False(await service.CanAddAsync(rome.Id, ivan.Id));
        }

        [Fact]
        public async Task CanAddAsync_MissingOrDeletedDestination_ReturnsFalse()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            rome.IsDeleted = true;
            await dbContext.SaveChangesAsync();

            Assert.False(await service.CanAddAsync(rome.Id, maria.Id));
            Assert.False(await service.CanAddAsync(999, maria.Id));
        }

        [Fact]
        public async Task CreateAsync_Expert_SavesTrimmedTip()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var before = DateTime.UtcNow;

            var result = await service.CreateAsync(
                rome.Id, CreateModel("   Book the Colosseum for the first slot of the day.  "), maria.Id);

            Assert.Equal(ExpertTipResult.Success, result);
            var tip = await dbContext.ExpertTips.SingleAsync();
            Assert.Equal("Book the Colosseum for the first slot of the day.", tip.Content);
            Assert.Equal(maria.Id, tip.AuthorId);
            Assert.Equal(rome.Id, tip.DestinationId);
            Assert.Null(tip.ModifiedOn);
            Assert.InRange(tip.CreatedOn, before, DateTime.UtcNow);
        }

        [Fact]
        public async Task CreateAsync_RegularUser_ReturnsNotExpertAndSavesNothing()
        {
            var (rome, _) = await AddDestinationsAsync();
            await AddExpertAsync("maria@voyage.test");
            var ivan = await AddUserAsync(dbContext, "ivan@voyage.test");

            var result = await service.CreateAsync(
                rome.Id, CreateModel("Avoid the restaurants right next to the Trevi Fountain."), ivan.Id);

            Assert.Equal(ExpertTipResult.NotExpert, result);
            Assert.Empty(dbContext.ExpertTips);
        }

        [Fact]
        public async Task CreateAsync_ExpertRoleWasRemoved_ReturnsNotExpert()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            await RemoveExpertAsync(maria);

            var result = await service.CreateAsync(
                rome.Id, CreateModel("Book the Colosseum for the first slot of the day."), maria.Id);

            Assert.Equal(ExpertTipResult.NotExpert, result);
        }

        [Fact]
        public async Task CreateAsync_SecondTipForSameDestination_ReturnsAlreadyExists()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            await service.CreateAsync(rome.Id, CreateModel("Book the Colosseum for the first slot of the day."), maria.Id);

            var result = await service.CreateAsync(
                rome.Id, CreateModel("Trastevere is the best area for dinner, away from the crowds."), maria.Id);

            Assert.Equal(ExpertTipResult.AlreadyExists, result);
            Assert.Single(dbContext.ExpertTips);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(999)]
        public async Task CreateAsync_MissingDestination_ReturnsDestinationNotFound(int destinationId)
        {
            var maria = await AddExpertAsync("maria@voyage.test");

            var result = await service.CreateAsync(
                destinationId, CreateModel("Book the Colosseum for the first slot of the day."), maria.Id);

            Assert.Equal(ExpertTipResult.DestinationNotFound, result);
        }

        [Fact]
        public async Task CreateAsync_DeletedDestination_ReturnsDestinationNotFound()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            rome.IsDeleted = true;
            await dbContext.SaveChangesAsync();

            var result = await service.CreateAsync(
                rome.Id, CreateModel("Book the Colosseum for the first slot of the day."), maria.Id);

            Assert.Equal(ExpertTipResult.DestinationNotFound, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateAndCanAdd_EmptyUserId_Throw(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() =>
                service.CreateAsync(1, CreateModel("Book the Colosseum for the first slot of the day."), userId!));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.CanAddAsync(1, userId!));
        }

        [Fact]
        public async Task GetForEditAsync_OwnTip_ReturnsTipWithDestination()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            var result = await service.GetForEditAsync(tip.Id, maria.Id);

            Assert.NotNull(result);
            Assert.Equal(rome.Id, result.DestinationId);
            Assert.Equal("Rome", result.DestinationName);
            Assert.Equal("Book the Colosseum for the first slot of the day.", result.Content);
        }

        [Fact]
        public async Task GetForEditAsync_TipOfAnotherUser_ReturnsNull()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var georgi = await AddExpertAsync("georgi@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            Assert.Null(await service.GetForEditAsync(tip.Id, georgi.Id));
        }

        [Fact]
        public async Task EditAsync_OwnTip_UpdatesContentAndModifiedDate()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            var result = await service.EditAsync(
                tip.Id, CreateModel("  Book the Colosseum online and go at 8:30 sharp.  "), maria.Id);

            Assert.Equal(rome.Id, result);
            var saved = await dbContext.ExpertTips.AsNoTracking().SingleAsync();
            Assert.Equal("Book the Colosseum online and go at 8:30 sharp.", saved.Content);
            Assert.NotNull(saved.ModifiedOn);
        }

        [Fact]
        public async Task EditAsync_TipOfAnotherUser_ReturnsNullAndKeepsContent()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var georgi = await AddExpertAsync("georgi@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            var result = await service.EditAsync(tip.Id, CreateModel("Rome is overrated, skip it completely."), georgi.Id);

            Assert.Null(result);
            Assert.Equal(
                "Book the Colosseum for the first slot of the day.",
                (await dbContext.ExpertTips.AsNoTracking().SingleAsync()).Content);
        }

        [Fact]
        public async Task EditAsync_MissingTip_ReturnsNull()
        {
            var maria = await AddExpertAsync("maria@voyage.test");

            Assert.Null(await service.EditAsync(999, CreateModel("Book the Colosseum for the first slot of the day."), maria.Id));
        }

        [Fact]
        public async Task DeleteAsync_OwnTip_RemovesItAndReturnsDestinationId()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            var result = await service.DeleteAsync(tip.Id, maria.Id);

            Assert.Equal(rome.Id, result);
            Assert.Empty(dbContext.ExpertTips);
        }

        [Fact]
        public async Task DeleteAsync_TipOfAnotherUser_ReturnsNullAndKeepsIt()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var georgi = await AddExpertAsync("georgi@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            Assert.Null(await service.DeleteAsync(tip.Id, georgi.Id));
            Assert.Single(dbContext.ExpertTips);
        }

        [Fact]
        public async Task DeleteAsync_ThenCanAddAgain()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            await service.DeleteAsync(tip.Id, maria.Id);

            Assert.True(await service.CanAddAsync(rome.Id, maria.Id));
        }

        [Fact]
        public async Task DeleteByModeratorAsync_AnyTip_RemovesItAndReturnsDestinationId()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            var result = await service.DeleteByModeratorAsync(tip.Id);

            Assert.Equal(rome.Id, result);
            Assert.Empty(dbContext.ExpertTips);
        }

        [Fact]
        public async Task DeleteByModeratorAsync_RemovesOnlyTheSelectedTip()
        {
            var (rome, paris) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var georgi = await AddExpertAsync("georgi@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");
            await AddTipAsync(georgi, rome, "Trastevere is the best area for dinner, away from the crowds.");
            await AddTipAsync(maria, paris, "Climb the Arc de Triomphe instead of the Eiffel Tower.");

            await service.DeleteByModeratorAsync(tip.Id);

            Assert.Equal(2, await dbContext.ExpertTips.CountAsync());
            Assert.DoesNotContain(dbContext.ExpertTips, t => t.Id == tip.Id);
        }

        [Fact]
        public async Task DeleteByModeratorAsync_MissingTip_ReturnsNull()
        {
            Assert.Null(await service.DeleteByModeratorAsync(999));
        }

        [Fact]
        public async Task DeleteByModeratorAsync_SameTipTwice_SecondTimeReturnsNull()
        {
            var (rome, _) = await AddDestinationsAsync();
            var maria = await AddExpertAsync("maria@voyage.test");
            var tip = await AddTipAsync(maria, rome, "Book the Colosseum for the first slot of the day.");

            await service.DeleteByModeratorAsync(tip.Id);

            Assert.Null(await service.DeleteByModeratorAsync(tip.Id));
        }

        private async Task<(Destination Rome, Destination Paris)> AddDestinationsAsync()
        {
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");

            return (rome, paris);
        }

        private async Task<IdentityUser> AddExpertAsync(string email)
        {
            var user = await AddUserAsync(dbContext, email);
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.TravelExpert);

            if (role == null)
            {
                role = new IdentityRole(RoleConstants.TravelExpert);
                dbContext.Roles.Add(role);
            }

            dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
            await dbContext.SaveChangesAsync();

            return user;
        }

        private async Task RemoveExpertAsync(IdentityUser user)
        {
            var userRole = await dbContext.UserRoles.SingleAsync(ur => ur.UserId == user.Id);
            dbContext.UserRoles.Remove(userRole);
            await dbContext.SaveChangesAsync();
        }

        private async Task<ExpertTip> AddTipAsync(IdentityUser author, Destination destination, string content, int daysAgo = 0)
        {
            var tip = new ExpertTip
            {
                AuthorId = author.Id,
                DestinationId = destination.Id,
                Content = content,
                CreatedOn = DateTime.UtcNow.AddDays(-daysAgo)
            };

            dbContext.ExpertTips.Add(tip);
            await dbContext.SaveChangesAsync();

            return tip;
        }

        private static ExpertTipFormModel CreateModel(string content)
        {
            return new ExpertTipFormModel { Content = content };
        }
    }
}
