using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Common.Constants;
using Voyage.Data.Models;

namespace Voyage.Data.Seeding
{
    public class ExpertTipSeeder : ISeeder
    {
        private static readonly SeedTip[] Tips =
        {
            new("maria@voyage.test", "Rome", 35,
                "Book the Colosseum for the first slot of the day and buy the combined ticket online. The Forum and Palatine Hill are much calmer after lunch."),
            new("maria@voyage.test", "Santorini", 38,
                "Stay in Firostefani instead of Oia. It is a short walk away, much cheaper and you still get the caldera view without the crowds."),
            new("maria@voyage.test", "Plitvice Lakes", 70,
                "Start at Entrance 2 and follow route H early in the morning. By 11:00 the wooden paths around the big waterfall are completely packed."),
            new("maria@voyage.test", "Barcelona", 110,
                "Get tickets for Sagrada Familia at least a week ahead and keep your phone in a front pocket on La Rambla and in the metro."),
            new("maria@voyage.test", "Prague", 58,
                "Cross Charles Bridge before 8 in the morning. For dinner skip the Old Town Square restaurants and walk ten minutes to Vinohrady."),
            new("georgi@voyage.test", "Bansko", 55,
                "Buy the lift pass online the evening before and take the first gondola. The queue at the bottom station after 9:00 can take an hour."),
            new("georgi@voyage.test", "Sozopol", 25,
                "Park near the new town and walk to the old town. In July and August the streets near the harbour are closed to cars after 18:00."),
            new("georgi@voyage.test", "Kyoto", 90,
                "Visit Fushimi Inari at sunrise and walk past the first viewpoint. Most people turn back there and the upper part is almost empty."),
            new("georgi@voyage.test", "Zermatt", 140,
                "The town is car free, so leave the car in Tasch and take the shuttle train. Check the Matterhorn webcam before buying a Gornergrat ticket."),
            new("georgi@voyage.test", "Istanbul", 45,
                "Get an Istanbulkart at the airport. It works on the metro, trams and ferries, and the ferry to the Asian side is the cheapest Bosphorus tour.")
        };

        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (await dbContext.ExpertTips.IgnoreQueryFilters().AnyAsync())
            {
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var destinationIds = (await dbContext.Destinations
                    .Where(d => !d.IsDeleted)
                    .Select(d => new { d.Id, d.Name })
                    .ToListAsync())
                .GroupBy(d => d.Name)
                .ToDictionary(g => g.Key, g => g.Min(d => d.Id));

            var experts = new Dictionary<string, IdentityUser?>();

            foreach (var seedTip in Tips)
            {
                if (!destinationIds.TryGetValue(seedTip.Destination, out var destinationId))
                {
                    continue;
                }

                if (!experts.TryGetValue(seedTip.AuthorEmail, out var author))
                {
                    author = await userManager.FindByEmailAsync(seedTip.AuthorEmail);

                    if (author != null && !await userManager.IsInRoleAsync(author, RoleConstants.TravelExpert))
                    {
                        author = null;
                    }

                    experts[seedTip.AuthorEmail] = author;
                }

                if (author == null)
                {
                    continue;
                }

                dbContext.ExpertTips.Add(new ExpertTip
                {
                    AuthorId = author.Id,
                    DestinationId = destinationId,
                    Content = seedTip.Content,
                    CreatedOn = DateTime.UtcNow.AddDays(-seedTip.DaysAgo)
                });
            }

            await dbContext.SaveChangesAsync();
        }

        private record SeedTip(string AuthorEmail, string Destination, int DaysAgo, string Content);
    }
}
