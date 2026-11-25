using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Common.Constants;
using Voyage.Data;
using Voyage.Data.Seeding;
using Voyage.Services.Tests.Infrastructure;
using Xunit;

using static Voyage.Services.Tests.Infrastructure.TestData;

namespace Voyage.Services.Tests
{
    public class TravelExpertServiceTests : IAsyncLifetime
    {
        private const string TestPassword = "Test-Pass1";

        private readonly ApplicationDbContext dbContext;
        private readonly ServiceProvider provider;
        private readonly UserManager<IdentityUser> userManager;
        private readonly TravelExpertService service;

        public TravelExpertServiceTests()
        {
            dbContext = TestDbContextFactory.Create();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(dbContext);
            services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            provider = services.BuildServiceProvider();
            userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            service = new TravelExpertService(dbContext, userManager);
        }

        public async Task InitializeAsync()
        {
            await new RolesSeeder().SeedAsync(dbContext, provider);
        }

        public Task DisposeAsync()
        {
            provider.Dispose();
            dbContext.Dispose();

            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsUsersWithStatusAndReviewsCount()
        {
            var maria = await CreateUserAsync("maria@voyage.test", RoleConstants.User, RoleConstants.TravelExpert);
            var ivan = await CreateUserAsync("ivan@voyage.test", RoleConstants.User);
            var category = await AddCategoryAsync(dbContext, "Culture");
            var rome = await AddDestinationAsync(dbContext, category.Id, "Rome", "Italy");
            var paris = await AddDestinationAsync(dbContext, category.Id, "Paris", "France");
            await AddReviewAsync(dbContext, maria.Id, rome.Id, 5);
            await AddReviewAsync(dbContext, maria.Id, paris.Id, 4);
            await AddReviewAsync(dbContext, ivan.Id, rome.Id, 3);

            var result = await service.GetUsersAsync(null);

            var users = result.Users.ToList();
            Assert.Equal(2, users.Count);
            Assert.Equal(1, result.ExpertsCount);
            Assert.True(users.Single(u => u.Id == maria.Id).IsExpert);
            Assert.Equal(2, users.Single(u => u.Id == maria.Id).ReviewsCount);
            Assert.False(users.Single(u => u.Id == ivan.Id).IsExpert);
            Assert.Equal(1, users.Single(u => u.Id == ivan.Id).ReviewsCount);
        }

        [Fact]
        public async Task GetUsersAsync_ExpertsFirstThenByEmail()
        {
            await CreateUserAsync("petya@voyage.test", RoleConstants.User);
            await CreateUserAsync("stefan@voyage.test", RoleConstants.User, RoleConstants.TravelExpert);
            await CreateUserAsync("ana@voyage.test", RoleConstants.User);
            await CreateUserAsync("georgi@voyage.test", RoleConstants.User, RoleConstants.TravelExpert);

            var result = await service.GetUsersAsync(null);

            Assert.Equal(
                new[] { "georgi@voyage.test", "stefan@voyage.test", "ana@voyage.test", "petya@voyage.test" },
                result.Users.Select(u => u.Email));
        }

        [Fact]
        public async Task GetUsersAsync_DoesNotListAdministrators()
        {
            await CreateUserAsync("admin@voyage.test", RoleConstants.Administrator);
            await CreateUserAsync("moderator@voyage.test", RoleConstants.Moderator);
            await CreateUserAsync("demo@voyage.test", RoleConstants.User);

            var result = await service.GetUsersAsync(null);

            Assert.Equal(
                new[] { "demo@voyage.test", "moderator@voyage.test" },
                result.Users.Select(u => u.Email));
        }

        [Theory]
        [InlineData("maria")]
        [InlineData("  MARIA ")]
        [InlineData("ria@voy")]
        public async Task GetUsersAsync_Search_FiltersByEmailIgnoringCase(string search)
        {
            await CreateUserAsync("maria@voyage.test", RoleConstants.User);
            await CreateUserAsync("ivan@voyage.test", RoleConstants.User);

            var result = await service.GetUsersAsync(search);

            Assert.Equal("maria@voyage.test", Assert.Single(result.Users).Email);
            Assert.Equal(search.Trim(), result.Search);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetUsersAsync_EmptySearch_ReturnsEveryoneAndNoSearchText(string? search)
        {
            await CreateUserAsync("maria@voyage.test", RoleConstants.User);
            await CreateUserAsync("ivan@voyage.test", RoleConstants.User);

            var result = await service.GetUsersAsync(search);

            Assert.Equal(2, result.Users.Count());
            Assert.Null(result.Search);
        }

        [Fact]
        public async Task GetUsersAsync_NoMatch_ReturnsEmptyList()
        {
            await CreateUserAsync("maria@voyage.test", RoleConstants.User);

            var result = await service.GetUsersAsync("nobody");

            Assert.Empty(result.Users);
        }

        [Fact]
        public async Task GrantAsync_RegularUser_BecomesExpertAndKeepsUserRole()
        {
            var ivan = await CreateUserAsync("ivan@voyage.test", RoleConstants.User);

            var result = await service.GrantAsync(ivan.Id);

            Assert.Equal("ivan@voyage.test", result);
            var roles = await userManager.GetRolesAsync(ivan);
            Assert.Contains(RoleConstants.TravelExpert, roles);
            Assert.Contains(RoleConstants.User, roles);
        }

        [Fact]
        public async Task GrantAsync_AlreadyExpert_ReturnsNullAndKeepsOneRole()
        {
            var maria = await CreateUserAsync("maria@voyage.test", RoleConstants.User, RoleConstants.TravelExpert);

            var result = await service.GrantAsync(maria.Id);

            Assert.Null(result);
            Assert.Single(await userManager.GetUsersInRoleAsync(RoleConstants.TravelExpert));
        }

        [Fact]
        public async Task GrantAsync_Administrator_IsNotChanged()
        {
            var admin = await CreateUserAsync("admin@voyage.test", RoleConstants.Administrator);

            var result = await service.GrantAsync(admin.Id);

            Assert.Null(result);
            Assert.False(await userManager.IsInRoleAsync(admin, RoleConstants.TravelExpert));
        }

        [Fact]
        public async Task GrantAsync_MissingUser_ReturnsNull()
        {
            Assert.Null(await service.GrantAsync("missing-user-id"));
        }

        [Fact]
        public async Task RemoveAsync_Expert_LosesOnlyTheExpertRole()
        {
            var maria = await CreateUserAsync("maria@voyage.test", RoleConstants.User, RoleConstants.TravelExpert);

            var result = await service.RemoveAsync(maria.Id);

            Assert.Equal("maria@voyage.test", result);
            Assert.Equal(new[] { RoleConstants.User }, await userManager.GetRolesAsync(maria));
        }

        [Fact]
        public async Task RemoveAsync_UserWhoIsNotExpert_ReturnsNull()
        {
            var ivan = await CreateUserAsync("ivan@voyage.test", RoleConstants.User);

            Assert.Null(await service.RemoveAsync(ivan.Id));
            Assert.Equal(new[] { RoleConstants.User }, await userManager.GetRolesAsync(ivan));
        }

        [Fact]
        public async Task RemoveAsync_MissingUser_ReturnsNull()
        {
            Assert.Null(await service.RemoveAsync("missing-user-id"));
        }

        [Fact]
        public async Task GrantThenRemove_UpdatesExpertsCount()
        {
            var ivan = await CreateUserAsync("ivan@voyage.test", RoleConstants.User);

            await service.GrantAsync(ivan.Id);
            var afterGrant = await service.GetUsersAsync(null);
            await service.RemoveAsync(ivan.Id);
            var afterRemove = await service.GetUsersAsync(null);

            Assert.Equal(1, afterGrant.ExpertsCount);
            Assert.Equal(0, afterRemove.ExpertsCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GrantAndRemove_EmptyUserId_Throw(string? userId)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.GrantAsync(userId!));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => service.RemoveAsync(userId!));
        }

        private async Task<IdentityUser> CreateUserAsync(string email, params string[] roles)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            await userManager.CreateAsync(user, TestPassword);

            foreach (var role in roles)
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }
    }
}
