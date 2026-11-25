using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Common.Constants;
using Voyage.Common.Constants.Validation;
using Voyage.Data;
using Voyage.Data.Models;
using Voyage.Data.Seeding;
using Voyage.Services.Tests.Infrastructure;
using Xunit;

namespace Voyage.Services.Tests
{
    public class SeedingTests : IDisposable
    {
        private const string TestPassword = "Test-Pass1";

        private static readonly string[] DemoEmails =
        {
            "demo@voyage.test",
            "maria@voyage.test",
            "georgi@voyage.test",
            "ivan@voyage.test",
            "elena@voyage.test",
            "nikolay@voyage.test",
            "petya@voyage.test",
            "stefan@voyage.test",
            "ana@voyage.test",
            "dimitar@voyage.test"
        };

        private readonly ApplicationDbContext dbContext;
        private readonly ApplicationDbContextSeeder seeder = new();

        public SeedingTests()
        {
            dbContext = TestDbContextFactory.Create();
            dbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        [Fact]
        public async Task SeedAsync_NullDbContext_Throws()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await Assert.ThrowsAsync<ArgumentNullException>(() => seeder.SeedAsync(null!, provider));
        }

        [Fact]
        public async Task SeedAsync_NullServiceProvider_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => seeder.SeedAsync(dbContext, null!));
        }

        [Fact]
        public async Task SeedAsync_CreatesAllRoles()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var roles = await dbContext.Roles.Select(r => r.Name).OrderBy(n => n).ToListAsync();
            Assert.Equal(
                new[] { RoleConstants.Administrator, RoleConstants.Moderator, RoleConstants.TravelExpert, RoleConstants.User },
                roles);
        }

        [Fact]
        public async Task SeedAsync_CreatesAdministratorAndModeratorInTheirRoles()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            var admin = await userManager.FindByEmailAsync("admin@voyage.test");
            var moderator = await userManager.FindByEmailAsync("moderator@voyage.test");

            Assert.NotNull(admin);
            Assert.NotNull(moderator);
            Assert.True(admin.EmailConfirmed);
            Assert.Equal(new[] { RoleConstants.Administrator }, await userManager.GetRolesAsync(admin));
            Assert.Equal(new[] { RoleConstants.Moderator }, await userManager.GetRolesAsync(moderator));
            Assert.True(await userManager.CheckPasswordAsync(admin, TestPassword));
        }

        [Fact]
        public async Task SeedAsync_CreatesAllDemoUsersInUserRole()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var email in DemoEmails)
            {
                var user = await userManager.FindByEmailAsync(email);

                Assert.NotNull(user);
                Assert.Equal(new[] { RoleConstants.User }, await userManager.GetRolesAsync(user));
            }

            Assert.Equal(DemoEmails.Length + 2, await dbContext.Users.CountAsync());
        }

        [Fact]
        public async Task SeedAsync_CreatesTripsWithExpensesAndPackingItems()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            Assert.Equal(21, await dbContext.Trips.CountAsync());
            Assert.Equal(103, await dbContext.Expenses.CountAsync());
            Assert.Equal(155, await dbContext.PackingItems.CountAsync());
        }

        [Fact]
        public async Task SeedAsync_EveryDemoUserHasAtLeastTwoTrips()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var owners = await dbContext.Trips.Select(t => t.Owner.Email!).ToListAsync();
            var tripsPerUser = owners.GroupBy(e => e).ToDictionary(g => g.Key, g => g.Count());

            Assert.All(DemoEmails, email => Assert.True(tripsPerUser[email] >= 2));
            Assert.Equal(3, tripsPerUser["demo@voyage.test"]);
            Assert.False(tripsPerUser.ContainsKey("admin@voyage.test"));
        }

        [Fact]
        public async Task SeedAsync_TripsHaveValidDatesBudgetsAndDestinations()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var trips = await dbContext.Trips
                .Include(t => t.TripDestinations)
                .Include(t => t.Expenses)
                .ToListAsync();

            Assert.All(trips, t =>
            {
                Assert.True(t.EndDate >= t.StartDate);
                Assert.True(t.Budget > 0);
                Assert.NotEmpty(t.TripDestinations);
                Assert.All(t.Expenses, e =>
                {
                    Assert.True(e.Amount > 0);
                    Assert.InRange(e.Date, t.StartDate, t.EndDate);
                });
            });
        }

        [Fact]
        public async Task SeedAsync_HasUpcomingOngoingAndCompletedTrips()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var today = DateTime.Today;
            var trips = await dbContext.Trips.ToListAsync();

            Assert.Contains(trips, t => t.StartDate > today);
            Assert.Contains(trips, t => t.EndDate < today);
            var ongoing = Assert.Single(trips, t => t.StartDate <= today && t.EndDate >= today);
            Assert.Equal("Istanbul Food Week", ongoing.Title);
        }

        [Fact]
        public async Task SeedAsync_CompletedTripsAreFullyPacked()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var today = DateTime.Today;
            var completedTrips = await dbContext.Trips
                .Include(t => t.PackingItems)
                .Where(t => t.EndDate < today)
                .ToListAsync();

            Assert.NotEmpty(completedTrips);
            Assert.All(completedTrips, t => Assert.All(t.PackingItems, p => Assert.True(p.IsPacked)));
        }

        [Fact]
        public async Task SeedAsync_PackingItemsAreValidAndUniquePerTrip()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var items = await dbContext.PackingItems.ToListAsync();

            Assert.All(items, p => Assert.InRange(
                p.Quantity,
                PackingItemValidationConstants.QuantityMin,
                PackingItemValidationConstants.QuantityMax));
            Assert.All(items.GroupBy(p => p.TripId), g =>
                Assert.Equal(g.Count(), g.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()));
            Assert.All(items.GroupBy(p => p.TripId), g =>
                Assert.True(g.Count() <= PackingItemValidationConstants.MaxPerTrip));
        }

        [Fact]
        public async Task SeedAsync_CreatesReviewsWithValidRatingsAndPastDates()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var reviews = await dbContext.Reviews.ToListAsync();

            Assert.Equal(52, reviews.Count);
            Assert.All(reviews, r =>
            {
                Assert.InRange(r.Rating, 1, 5);
                Assert.False(string.IsNullOrWhiteSpace(r.Comment));
                Assert.True(r.CreatedOn <= DateTime.UtcNow);
            });
            Assert.Contains(reviews, r => r.Rating == 1);
        }

        [Fact]
        public async Task SeedAsync_EachUserReviewsADestinationAtMostOnce()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var pairs = await dbContext.Reviews.Select(r => new { r.AuthorId, r.DestinationId }).ToListAsync();

            Assert.Equal(pairs.Count, pairs.Distinct().Count());
        }

        [Theory]
        [InlineData("Rome", 5, 4.8)]
        [InlineData("Plitvice Lakes", 3, 4.67)]
        [InlineData("Borovets", 3, 2.67)]
        public async Task SeedAsync_DestinationsHaveExpectedRatings(string destination, int expectedCount, double expectedAverage)
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var ratings = await dbContext.Reviews
                .Where(r => r.Destination.Name == destination)
                .Select(r => r.Rating)
                .ToListAsync();

            Assert.Equal(expectedCount, ratings.Count);
            Assert.Equal(expectedAverage, Math.Round(ratings.Average(), 2));
        }

        [Fact]
        public async Task SeedAsync_CreatesFavoritesForEveryDemoUser()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var users = await dbContext.Favorites.Select(f => f.User.Email!).ToListAsync();
            var favoritesPerUser = users.GroupBy(e => e).ToDictionary(g => g.Key, g => g.Count());

            Assert.Equal(43, favoritesPerUser.Values.Sum());
            Assert.All(DemoEmails, email => Assert.InRange(favoritesPerUser[email], 3, 6));
        }

        [Fact]
        public async Task SeedAsync_RomeIsTheMostSavedDestination()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            var names = await dbContext.Favorites.Select(f => f.Destination.Name).ToListAsync();
            var counts = names.GroupBy(n => n).Select(g => new { Name = g.Key, Count = g.Count() }).ToList();

            Assert.Equal(4, counts.Single(c => c.Name == "Rome").Count);
            Assert.Equal(4, counts.Max(c => c.Count));
        }

        [Fact]
        public async Task SeedAsync_RunTwice_DoesNotDuplicateAnything()
        {
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);
            var before = await CountEverythingAsync();

            await seeder.SeedAsync(dbContext, provider);
            var after = await CountEverythingAsync();

            Assert.Equal(before, after);
        }

        [Fact]
        public async Task SeedAsync_WithoutConfiguredUsers_CreatesOnlyRoles()
        {
            using var provider = BuildServiceProvider(new Dictionary<string, string?>());

            await seeder.SeedAsync(dbContext, provider);

            Assert.Equal(4, await dbContext.Roles.CountAsync());
            Assert.Empty(dbContext.Users);
            Assert.Empty(dbContext.Trips);
            Assert.Empty(dbContext.Reviews);
            Assert.Empty(dbContext.Favorites);
        }

        [Theory]
        [InlineData("", TestPassword)]
        [InlineData("admin@voyage.test", "")]
        [InlineData("admin@voyage.test", "123")]
        public async Task SeedAsync_MissingOrInvalidAdminSettings_DoesNotCreateAdmin(string email, string password)
        {
            var settings = FullSettings();
            settings["AdminUser:Email"] = email;
            settings["AdminUser:Password"] = password;
            using var provider = BuildServiceProvider(settings);

            await seeder.SeedAsync(dbContext, provider);

            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            Assert.Empty(await userManager.GetUsersInRoleAsync(RoleConstants.Administrator));
            Assert.NotNull(await userManager.FindByEmailAsync("demo@voyage.test"));
        }

        [Fact]
        public async Task SeedAsync_ExistingUserWithoutRole_GetsTheConfiguredRole()
        {
            using var provider = BuildServiceProvider(FullSettings());
            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            var existing = new IdentityUser { UserName = "moderator@voyage.test", Email = "moderator@voyage.test" };
            await userManager.CreateAsync(existing, TestPassword);

            await seeder.SeedAsync(dbContext, provider);

            Assert.Single(dbContext.Users, u => u.Email == "moderator@voyage.test");
            Assert.True(await userManager.IsInRoleAsync(existing, RoleConstants.Moderator));
        }

        [Fact]
        public async Task SeedAsync_RegisteredUserWithoutRole_GetsUserRole()
        {
            using var provider = BuildServiceProvider(FullSettings());
            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            var registered = new IdentityUser { UserName = "new.traveller@voyage.test", Email = "new.traveller@voyage.test" };
            await userManager.CreateAsync(registered, TestPassword);

            await seeder.SeedAsync(dbContext, provider);

            Assert.Equal(new[] { RoleConstants.User }, await userManager.GetRolesAsync(registered));
        }

        [Fact]
        public async Task SeedAsync_UserThatAlreadyHasTrips_DoesNotGetSeededTrips()
        {
            using var provider = BuildServiceProvider(FullSettings());
            await new RolesSeeder().SeedAsync(dbContext, provider);
            await new DemoUsersSeeder().SeedAsync(dbContext, provider);
            var demo = await dbContext.Users.SingleAsync(u => u.Email == "demo@voyage.test");
            await TestData.AddTripAsync(dbContext, demo.Id, "My own trip");

            await seeder.SeedAsync(dbContext, provider);

            var demoTrips = await dbContext.Trips.Where(t => t.OwnerId == demo.Id).Select(t => t.Title).ToListAsync();
            Assert.Equal(new[] { "My own trip" }, demoTrips);
            Assert.Equal(21 - 3 + 1, await dbContext.Trips.CountAsync());
        }

        [Fact]
        public async Task SeedAsync_ExistingReview_IsKeptAndNotDuplicated()
        {
            using var provider = BuildServiceProvider(FullSettings());
            await new RolesSeeder().SeedAsync(dbContext, provider);
            await new DemoUsersSeeder().SeedAsync(dbContext, provider);
            var maria = await dbContext.Users.SingleAsync(u => u.Email == "maria@voyage.test");
            var santorini = await dbContext.Destinations.SingleAsync(d => d.Name == "Santorini");
            await TestData.AddReviewAsync(dbContext, maria.Id, santorini.Id, 2);

            await seeder.SeedAsync(dbContext, provider);

            var review = await dbContext.Reviews.SingleAsync(r => r.AuthorId == maria.Id && r.DestinationId == santorini.Id);
            Assert.Equal(2, review.Rating);
            Assert.Equal(52, await dbContext.Reviews.CountAsync());
        }

        [Fact]
        public async Task SeedAsync_DeletedReview_IsNotRecreated()
        {
            using var provider = BuildServiceProvider(FullSettings());
            await new RolesSeeder().SeedAsync(dbContext, provider);
            await new DemoUsersSeeder().SeedAsync(dbContext, provider);
            var maria = await dbContext.Users.SingleAsync(u => u.Email == "maria@voyage.test");
            var santorini = await dbContext.Destinations.SingleAsync(d => d.Name == "Santorini");
            await TestData.AddReviewAsync(dbContext, maria.Id, santorini.Id, 5, isDeleted: true);

            await seeder.SeedAsync(dbContext, provider);

            Assert.Equal(51, await dbContext.Reviews.CountAsync());
            Assert.Equal(52, await dbContext.Reviews.IgnoreQueryFilters().CountAsync());
        }

        [Fact]
        public async Task SeedAsync_UserThatAlreadyHasFavorites_DoesNotGetSeededFavorites()
        {
            using var provider = BuildServiceProvider(FullSettings());
            await new RolesSeeder().SeedAsync(dbContext, provider);
            await new DemoUsersSeeder().SeedAsync(dbContext, provider);
            var demo = await dbContext.Users.SingleAsync(u => u.Email == "demo@voyage.test");
            var sozopol = await dbContext.Destinations.SingleAsync(d => d.Name == "Sozopol");
            dbContext.Favorites.Add(new Favorite { UserId = demo.Id, DestinationId = sozopol.Id, AddedOn = DateTime.UtcNow });
            await dbContext.SaveChangesAsync();

            await seeder.SeedAsync(dbContext, provider);

            var demoFavorites = await dbContext.Favorites.Where(f => f.UserId == demo.Id).ToListAsync();
            Assert.Single(demoFavorites);
            Assert.Equal(43 - 5 + 1, await dbContext.Favorites.CountAsync());
        }

        [Fact]
        public async Task SeedAsync_DeletedDestination_IsNotUsedInTripsReviewsOrFavorites()
        {
            var rome = await dbContext.Destinations.SingleAsync(d => d.Name == "Rome");
            rome.IsDeleted = true;
            await dbContext.SaveChangesAsync();
            using var provider = BuildServiceProvider(FullSettings());

            await seeder.SeedAsync(dbContext, provider);

            Assert.Empty(dbContext.TripDestinations.IgnoreQueryFilters().Where(td => td.DestinationId == rome.Id));
            Assert.Empty(dbContext.Reviews.IgnoreQueryFilters().Where(r => r.DestinationId == rome.Id));
            Assert.Empty(dbContext.Favorites.IgnoreQueryFilters().Where(f => f.DestinationId == rome.Id));
            Assert.Equal(52 - 5, await dbContext.Reviews.CountAsync());
        }

        private async Task<(int Users, int Roles, int UserRoles, int Trips, int Expenses, int Packing, int Reviews, int Favorites)> CountEverythingAsync()
        {
            return (
                await dbContext.Users.CountAsync(),
                await dbContext.Roles.CountAsync(),
                await dbContext.UserRoles.CountAsync(),
                await dbContext.Trips.CountAsync(),
                await dbContext.Expenses.CountAsync(),
                await dbContext.PackingItems.CountAsync(),
                await dbContext.Reviews.IgnoreQueryFilters().CountAsync(),
                await dbContext.Favorites.IgnoreQueryFilters().CountAsync());
        }

        private ServiceProvider BuildServiceProvider(Dictionary<string, string?> settings)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton(dbContext);
            services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            return services.BuildServiceProvider();
        }

        private static Dictionary<string, string?> FullSettings()
        {
            var settings = new Dictionary<string, string?>
            {
                ["AdminUser:Email"] = "admin@voyage.test",
                ["AdminUser:Password"] = TestPassword,
                ["ModeratorUser:Email"] = "moderator@voyage.test",
                ["ModeratorUser:Password"] = TestPassword
            };

            for (int i = 0; i < DemoEmails.Length; i++)
            {
                settings[$"DemoUsers:{i}:Email"] = DemoEmails[i];
                settings[$"DemoUsers:{i}:Password"] = TestPassword;
            }

            return settings;
        }
    }
}
