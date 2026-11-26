using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Data.Models;

namespace Voyage.Data.Seeding
{
    public class ReviewSeeder : ISeeder
    {
        private static readonly SeedReview[] Reviews =
        {
            new("maria@voyage.test", "Santorini", 5, 40,
                "Watched the sunset from Oia and it was every bit as beautiful as the photos. Go early to get a spot by the castle ruins, it gets packed an hour before sunset."),
            new("ivan@voyage.test", "Santorini", 4, 95,
                "Stunning views and great food, but prices in Fira are really high. Renting an ATV for a day was the best way to see the island."),
            new("petya@voyage.test", "Santorini", 3, 150,
                "Beautiful, but in August it felt like a theme park. Cruise ships everywhere. I would come back in May or October."),
            new("elena@voyage.test", "Santorini", 5, 12,
                "The black sand beach at Perissa was a nice surprise. Quiet, warm water and cheap sunbeds compared to the north."),
            new("georgi@voyage.test", "Bansko", 4, 60,
                "Good slopes for intermediate skiers and the old town is charming. The gondola queue in the morning can take 40 minutes, so start early."),
            new("dimitar@voyage.test", "Bansko", 5, 30,
                "Great snow this season. Try the mehanas in the old town after skiing, the kapama is a must."),
            new("ana@voyage.test", "Bansko", 3, 200,
                "Nice in winter, but in summer half of the places were closed. Still a good base for hiking in Pirin."),
            new("petya@voyage.test", "Paris", 5, 14,
                "Our first trip together and Paris did not disappoint. Book the Louvre online in advance, the queue without a ticket was huge."),
            new("stefan@voyage.test", "Paris", 4, 75,
                "Montmartre early in the morning is magical. The city is expensive and the metro is crowded, but walking along the Seine makes up for it."),
            new("nikolay@voyage.test", "Paris", 3, 180,
                "Some areas are amazing, others felt dirty and overcrowded. Musee d'Orsay was the highlight for me, much calmer than the Louvre."),
            new("demo@voyage.test", "Rome", 5, 20,
                "Every street feels like a museum. The Colosseum underground tour is worth the extra money. Eat away from the main squares, the food is cheaper and better."),
            new("maria@voyage.test", "Rome", 4, 110,
                "Amazing history and food. Very hot in July, plan the outdoor sights for the morning and keep the afternoons for churches and museums."),
            new("ana@voyage.test", "Plitvice Lakes", 5, 50,
                "The colour of the water is unreal. Take route C or H and arrive at opening time, by noon the wooden paths are full of tour groups."),
            new("ivan@voyage.test", "Plitvice Lakes", 4, 130,
                "Beautiful national park, but swimming is not allowed and the ticket price in summer is quite high. Still worth it."),
            new("elena@voyage.test", "Interlaken", 5, 65,
                "Paragliding over the lakes was the best thing I have done in years. The town itself is small, but the location is perfect for day trips."),
            new("demo@voyage.test", "Interlaken", 4, 140,
                "Great base for Jungfraujoch and the Lauterbrunnen valley. Switzerland is expensive, so buy food in Coop or Migros."),
            new("georgi@voyage.test", "Sozopol", 5, 85,
                "The old town with its wooden houses is lovely in the evening. Fresh fish at the harbour and calm beaches south of the town."),
            new("stefan@voyage.test", "Sozopol", 3, 160,
                "Nice old town, but in August it is very crowded and parking is a nightmare. June or September is a much better time."),
            new("maria@voyage.test", "Barcelona", 4, 70,
                "Sagrada Familia is breathtaking inside, book the tower ticket too. Watch your phone on La Rambla, pickpockets are real."),
            new("ivan@voyage.test", "Barcelona", 5, 25,
                "Went for the food and was not disappointed. La Boqueria for breakfast, tapas in El Born in the evening. Bunkers del Carmel for the sunset."),
            new("maria@voyage.test", "Prague", 5, 55,
                "Charles Bridge at 7 in the morning without the crowds is something special. Cheap and excellent beer, very walkable city."),
            new("dimitar@voyage.test", "Prague", 4, 190,
                "Beautiful old town, but the area around the astronomical clock is a tourist trap. Letna park has the best view and a nice beer garden."),
            new("georgi@voyage.test", "Kyoto", 5, 210,
                "Fushimi Inari at sunrise was unforgettable. The city is very clean and the trains are always on time. Stay at least four days."),
            new("nikolay@voyage.test", "Kyoto", 4, 240,
                "Arashiyama bamboo grove is smaller than I expected and very busy, but the temples around it are peaceful. Great food everywhere."),
            new("nikolay@voyage.test", "Banff", 5, 35,
                "Lake Louise and Moraine Lake look like postcards. Book the parks shuttle early, private cars can no longer drive to Moraine Lake."),
            new("dimitar@voyage.test", "Queenstown", 5, 120,
                "Adventure capital for a reason. Bungee jump, jet boat and a day trip to Milford Sound. Expensive, but every day was amazing."),
            new("petya@voyage.test", "Bali", 4, 80,
                "Ubud and the rice terraces were beautiful, the south was too busy for us. Traffic is terrible, plan fewer places per day."),
            new("ana@voyage.test", "Bali", 5, 170,
                "Friendly people, great yoga retreats and amazing fresh fruit. Nusa Penida is worth the rough boat ride."),
            new("stefan@voyage.test", "Velingrad", 4, 45,
                "Relaxing weekend with mineral pools and walks in the pine forest. Good value compared to the seaside resorts."),
            new("demo@voyage.test", "Velingrad", 5, 175,
                "Perfect place to recharge. We stayed in a hotel with its own mineral spring and the food was simple but tasty."),
            new("elena@voyage.test", "Istanbul", 5, 20,
                "Hagia Sophia, the Grand Bazaar and a boat trip on the Bosphorus in one weekend. Tiring, but absolutely worth it."),
            new("georgi@voyage.test", "Istanbul", 4, 100,
                "Fantastic food and history. Traffic is crazy, use the tram and the ferries instead of taxis."),
            new("ivan@voyage.test", "Rila Lakes", 5, 45,
                "Classic hike, but still beautiful every time. Take the lift up from Panichishte and start before 9 to avoid the crowds at the top."),
            new("maria@voyage.test", "Rila Lakes", 4, 230,
                "Amazing views from the top above the lakes. The trail is easy, but the weather changes quickly, bring a jacket even in July."),
            new("elena@voyage.test", "Reykjavik", 4, 125,
                "Small and cosy city, a good base for the Golden Circle. Everything is very expensive, especially eating out."),
            new("nikolay@voyage.test", "Reykjavik", 5, 260,
                "We saw the northern lights on our second night. Sky Lagoon was better than the Blue Lagoon for us and less crowded."),
            new("petya@voyage.test", "Budapest", 5, 90,
                "Szechenyi baths in the evening, ruin bars at night and the Parliament lit up from the river. Great city for a long weekend."),
            new("maria@voyage.test", "Budapest", 4, 58,
                "Beautiful architecture and good prices. Fishermen's Bastion is very busy, go there before 8 for photos."),
            new("ivan@voyage.test", "Lisbon", 5, 70,
                "Tram 28 is fun but packed, so ride it early in the morning. The sunset from Miradouro da Senhora do Monte was the highlight of the whole trip."),
            new("ana@voyage.test", "Lisbon", 4, 165,
                "Lovely city with great food and fair prices. The hills are steep and the cobblestones get slippery, so bring comfortable shoes. Sintra is worth a full day."),
            new("dimitar@voyage.test", "Borovets", 4, 30,
                "Good slopes and a lively atmosphere. The lift system is a bit old, but the ski schools are good for beginners."),
            new("stefan@voyage.test", "Borovets", 3, 220,
                "Fine for a weekend, but the pistes get icy in the afternoon and food on the slopes is overpriced."),
            new("ana@voyage.test", "Zermatt", 5, 140,
                "Waking up with a view of the Matterhorn is something else. Car-free village, excellent skiing and hiking. Very expensive though."),
            new("georgi@voyage.test", "Zermatt", 4, 250,
                "Gornergrat railway gives the best panorama. Bring your own snacks, a sandwich on the mountain costs a fortune."),
            new("nikolay@voyage.test", "Sozopol", 2, 95,
                "The old town is nice, but our hotel near the beach was noisy until 3 am every night and the beach was not cleaned. Expected more for the price."),
            new("dimitar@voyage.test", "Bali", 2, 260,
                "Got sick on the second day and spent half of the trip in the hotel. Drink only bottled water and be careful with street food."),
            new("elena@voyage.test", "Barcelona", 3, 210,
                "Great architecture, but I had my wallet stolen on the metro on the first day. Keep valuables in a front pocket."),
            new("elena@voyage.test", "Banff", 4, 190,
                "Incredible scenery and we saw elk right in town. Accommodation in Banff is very expensive, staying in Canmore saved us a lot."),
            new("ana@voyage.test", "Queenstown", 4, 75,
                "Beautiful lake and mountains, and the Fergburger lives up to the hype. A bit too touristy in the centre, but the day trips are amazing."),
            new("maria@voyage.test", "Plitvice Lakes", 5, 230,
                "We went in late September and it was perfect. Fewer people, autumn colours and the boat across Kozjak lake was included in the ticket."),
            new("georgi@voyage.test", "Rome", 5, 160,
                "Three days were not enough. Trastevere in the evening, the Pantheon at night and the best carbonara of my life near Campo de' Fiori."),
            new("elena@voyage.test", "Rome", 5, 65,
                "The Vatican Museums were overwhelming in the best way. Book the first entry slot, the Sistine Chapel is much calmer before 9."),
            new("petya@voyage.test", "Rome", 5, 240,
                "Romantic, delicious and full of surprises around every corner. We threw a coin in the Trevi Fountain, so we have to come back."),
            new("ivan@voyage.test", "Borovets", 1, 40,
                "Terrible experience. The main gondola was closed for three of our five days, nobody refunded the lift passes and the rental skis were in bad condition. Will not come back.")
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

            var existingReviews = await dbContext.Reviews
                .IgnoreQueryFilters()
                .Select(r => new { r.AuthorId, r.DestinationId })
                .ToListAsync();

            var reviewed = existingReviews
                .Select(r => (r.AuthorId, r.DestinationId))
                .ToHashSet();

            var authorIds = new Dictionary<string, string>();

            foreach (var seedReview in Reviews)
            {
                if (!destinationIds.TryGetValue(seedReview.Destination, out var destinationId))
                {
                    continue;
                }

                if (!authorIds.TryGetValue(seedReview.AuthorEmail, out var authorId))
                {
                    var author = await userManager.FindByEmailAsync(seedReview.AuthorEmail);

                    if (author == null)
                    {
                        continue;
                    }

                    authorId = author.Id;
                    authorIds[seedReview.AuthorEmail] = authorId;
                }

                if (!reviewed.Add((authorId, destinationId)))
                {
                    continue;
                }

                dbContext.Reviews.Add(new Review
                {
                    AuthorId = authorId,
                    DestinationId = destinationId,
                    Rating = seedReview.Rating,
                    Comment = seedReview.Comment,
                    CreatedOn = DateTime.UtcNow.AddDays(-seedReview.DaysAgo)
                });
            }

            await dbContext.SaveChangesAsync();
        }

        private sealed record SeedReview(
            string AuthorEmail,
            string Destination,
            int Rating,
            int DaysAgo,
            string Comment);
    }
}
