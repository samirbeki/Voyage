using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Data.Models;

namespace Voyage.Data.Seeding
{
    public class FavoriteSeeder : ISeeder
    {
        private static readonly SeedFavorites[] Favorites =
        {
            new("demo@voyage.test", new[] { "Rome", "Santorini", "Interlaken", "Zermatt", "Kyoto" }),
            new("maria@voyage.test", new[] { "Santorini", "Prague", "Budapest", "Barcelona", "Plitvice Lakes", "Bali" }),
            new("georgi@voyage.test", new[] { "Kyoto", "Sozopol", "Istanbul", "Queenstown" }),
            new("ivan@voyage.test", new[] { "Barcelona", "Rila Lakes", "Bansko" }),
            new("elena@voyage.test", new[] { "Reykjavik", "Istanbul", "Banff", "Rome" }),
            new("nikolay@voyage.test", new[] { "Banff", "Reykjavik", "Zermatt", "Kyoto", "Queenstown" }),
            new("petya@voyage.test", new[] { "Paris", "Bali", "Rome", "Santorini", "Velingrad" }),
            new("stefan@voyage.test", new[] { "Velingrad", "Sozopol", "Borovets" }),
            new("ana@voyage.test", new[] { "Plitvice Lakes", "Zermatt", "Bali", "Rome" }),
            new("dimitar@voyage.test", new[] { "Queenstown", "Borovets", "Bansko", "Interlaken" })
        };

        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var destinations = await dbContext.Destinations
                .Where(d => !d.IsDeleted)
                .Select(d => new { d.Id, d.Name })
                .ToListAsync();

            var destinationIds = destinations
                .GroupBy(d => d.Name)
                .ToDictionary(g => g.Key, g => g.Min(d => d.Id));

            foreach (var seedFavorites in Favorites)
            {
                var user = await userManager.FindByEmailAsync(seedFavorites.UserEmail);

                if (user == null || await dbContext.Favorites.IgnoreQueryFilters().AnyAsync(f => f.UserId == user.Id))
                {
                    continue;
                }

                var daysAgo = seedFavorites.Destinations.Length * 9;

                foreach (var destinationName in seedFavorites.Destinations)
                {
                    if (destinationIds.TryGetValue(destinationName, out var destinationId))
                    {
                        dbContext.Favorites.Add(new Favorite
                        {
                            UserId = user.Id,
                            DestinationId = destinationId,
                            AddedOn = DateTime.UtcNow.AddDays(-daysAgo)
                        });
                    }

                    daysAgo -= 9;
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private sealed record SeedFavorites(string UserEmail, string[] Destinations);
    }
}
