using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voyage.Data.Models;
using Voyage.Data.Models.Enums;

namespace Voyage.Data.Seeding
{
    public class TripSeeder : ISeeder
    {
        private static readonly SeedTrip[] Trips =
        {
            new("demo@voyage.test", "Summer in Italy",
                "A week of food, history and long walks around Rome.",
                17, 7, 1800m,
                new[] { "Rome" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Rome", 286.40m, ExpenseCategory.Transport, 0),
                    new("Hotel near Termini, 7 nights", 742.00m, ExpenseCategory.Accommodation, 0),
                    new("Colosseum underground tour", 72.00m, ExpenseCategory.Activities, 2),
                    new("Vatican Museums tickets", 40.00m, ExpenseCategory.Activities, 3),
                    new("Leonardo Express train", 28.00m, ExpenseCategory.Transport, 0),
                    new("Travel insurance", 19.90m, ExpenseCategory.Other, 0)
                },
                new SeedPackingItem[]
                {
                    new("Passport", 1, PackingCategory.Documents, false),
                    new("Comfortable walking shoes", 1, PackingCategory.Clothing, false),
                    new("Linen shirts", 3, PackingCategory.Clothing, false),
                    new("Shorts", 2, PackingCategory.Clothing, false),
                    new("Scarf for churches", 1, PackingCategory.Clothing, false),
                    new("Sunscreen SPF 50", 1, PackingCategory.Toiletries, false),
                    new("Toothbrush and toothpaste", 1, PackingCategory.Toiletries, false),
                    new("Power bank", 1, PackingCategory.Electronics, false),
                    new("Phone charger", 1, PackingCategory.Electronics, false),
                    new("Painkillers", 1, PackingCategory.Health, false)
                }),
            new("demo@voyage.test", "Alpine Winter Escape",
                "Skiing and mountain views in the Swiss Alps.",
                45, 6, 2500m,
                new[] { "Interlaken", "Zermatt" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Zurich", 318.00m, ExpenseCategory.Transport, 0),
                    new("Swiss Travel Pass, 8 days", 389.00m, ExpenseCategory.Transport, 0),
                    new("Hotel in Interlaken, 3 nights", 465.00m, ExpenseCategory.Accommodation, 0),
                    new("Chalet in Zermatt, 3 nights", 690.00m, ExpenseCategory.Accommodation, 3),
                    new("Ski equipment rental", 210.00m, ExpenseCategory.Activities, 3)
                },
                new SeedPackingItem[]
                {
                    new("Ski jacket", 1, PackingCategory.Clothing, false),
                    new("Ski pants", 1, PackingCategory.Clothing, false),
                    new("Thermal underwear", 2, PackingCategory.Clothing, false),
                    new("Wool socks", 4, PackingCategory.Clothing, false),
                    new("Gloves", 2, PackingCategory.Clothing, false),
                    new("Ski goggles", 1, PackingCategory.Other, false),
                    new("Lip balm with SPF", 1, PackingCategory.Toiletries, false),
                    new("Travel insurance", 1, PackingCategory.Documents, false),
                    new("Swiss plug adapter", 1, PackingCategory.Electronics, false),
                    new("Blister plasters", 1, PackingCategory.Health, true)
                }),
            new("demo@voyage.test", "Weekend in Bansko",
                "A short ski weekend in the Pirin mountains.",
                -30, 2, 400m,
                new[] { "Bansko" },
                new SeedExpense[]
                {
                    new("Fuel and tolls", 48.50m, ExpenseCategory.Transport, 0),
                    new("Guest house, 2 nights", 120.00m, ExpenseCategory.Accommodation, 0),
                    new("Lift pass, 2 days", 104.00m, ExpenseCategory.Activities, 0),
                    new("Ski rental", 50.00m, ExpenseCategory.Activities, 0),
                    new("Dinner at a mehana", 42.30m, ExpenseCategory.Food, 1),
                    new("Hot drinks on the slopes", 14.00m, ExpenseCategory.Food, 1)
                },
                new SeedPackingItem[]
                {
                    new("Ski pants", 1, PackingCategory.Clothing, true),
                    new("Warm socks", 3, PackingCategory.Clothing, true),
                    new("Helmet", 1, PackingCategory.Other, true),
                    new("ID card", 1, PackingCategory.Documents, true),
                    new("Phone charger", 1, PackingCategory.Electronics, true),
                    new("Sun cream", 1, PackingCategory.Toiletries, true)
                }),
            new("maria@voyage.test", "Greek Island Hopping",
                "Sunsets, beaches and white villages on Santorini.",
                60, 7, 2200m,
                new[] { "Santorini" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Santorini", 398.00m, ExpenseCategory.Transport, 0),
                    new("Hotel in Oia with caldera view", 1120.00m, ExpenseCategory.Accommodation, 0),
                    new("Catamaran sunset cruise", 135.00m, ExpenseCategory.Activities, 3),
                    new("Airport transfer", 35.00m, ExpenseCategory.Transport, 0)
                },
                new SeedPackingItem[]
                {
                    new("Swimsuit", 2, PackingCategory.Clothing, false),
                    new("Summer dresses", 4, PackingCategory.Clothing, false),
                    new("Sandals", 1, PackingCategory.Clothing, false),
                    new("Sunglasses", 1, PackingCategory.Other, false),
                    new("Beach bag", 1, PackingCategory.Other, false),
                    new("Sunscreen SPF 50", 2, PackingCategory.Toiletries, false),
                    new("After-sun lotion", 1, PackingCategory.Toiletries, false),
                    new("Camera", 1, PackingCategory.Electronics, false),
                    new("Passport", 1, PackingCategory.Documents, false),
                    new("Motion sickness tablets", 1, PackingCategory.Health, false)
                }),
            new("maria@voyage.test", "Prague and Budapest by Train",
                "Two capitals, old bridges and thermal baths.",
                -60, 7, 1500m,
                new[] { "Prague", "Budapest" },
                new SeedExpense[]
                {
                    new("Flight Sofia - Prague", 124.00m, ExpenseCategory.Transport, 0),
                    new("Apartment in Mala Strana, 3 nights", 255.00m, ExpenseCategory.Accommodation, 0),
                    new("Prague Castle ticket", 18.00m, ExpenseCategory.Activities, 1),
                    new("Czech dinner and beer", 36.50m, ExpenseCategory.Food, 1),
                    new("Train Prague - Budapest", 39.90m, ExpenseCategory.Transport, 3),
                    new("Hotel near Andrassy Avenue, 3 nights", 246.00m, ExpenseCategory.Accommodation, 3),
                    new("Szechenyi Baths ticket", 32.00m, ExpenseCategory.Activities, 5),
                    new("Food and coffee in Budapest", 178.10m, ExpenseCategory.Food, 6),
                    new("Souvenirs", 58.00m, ExpenseCategory.Shopping, 6),
                    new("Flight Budapest - Sofia", 96.00m, ExpenseCategory.Transport, 7)
                },
                new SeedPackingItem[]
                {
                    new("Rain jacket", 1, PackingCategory.Clothing, true),
                    new("Sneakers", 1, PackingCategory.Clothing, true),
                    new("Jeans", 2, PackingCategory.Clothing, true),
                    new("Swimsuit for the baths", 1, PackingCategory.Clothing, true),
                    new("Flip-flops", 1, PackingCategory.Clothing, true),
                    new("ID card", 1, PackingCategory.Documents, true),
                    new("Train tickets (printed)", 1, PackingCategory.Documents, true),
                    new("Headphones", 1, PackingCategory.Electronics, true),
                    new("Shampoo", 1, PackingCategory.Toiletries, true)
                }),
            new("georgi@voyage.test", "Black Sea Weekend",
                "A relaxed weekend in the old town of Sozopol.",
                -90, 2, 350m,
                new[] { "Sozopol" },
                new SeedExpense[]
                {
                    new("Bus Sofia - Burgas", 35.00m, ExpenseCategory.Transport, 0),
                    new("Taxi Burgas - Sozopol", 22.00m, ExpenseCategory.Transport, 0),
                    new("Room near the harbour, 2 nights", 140.00m, ExpenseCategory.Accommodation, 0),
                    new("Fish restaurant", 58.70m, ExpenseCategory.Food, 1),
                    new("Boat trip to St. Ivan island", 25.00m, ExpenseCategory.Activities, 1),
                    new("Ice cream and coffee", 16.40m, ExpenseCategory.Food, 2)
                },
                new SeedPackingItem[]
                {
                    new("Swim shorts", 2, PackingCategory.Clothing, true),
                    new("T-shirts", 3, PackingCategory.Clothing, true),
                    new("Towel", 1, PackingCategory.Other, true),
                    new("Sunscreen", 1, PackingCategory.Toiletries, true),
                    new("Book", 1, PackingCategory.Other, true),
                    new("ID card", 1, PackingCategory.Documents, true)
                }),
            new("georgi@voyage.test", "Japan in Spring",
                "Temples, gardens and cherry blossoms in Kyoto.",
                120, 12, 4500m,
                new[] { "Kyoto" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Osaka", 1180.00m, ExpenseCategory.Transport, 0),
                    new("Japan Rail Pass, 7 days", 320.00m, ExpenseCategory.Transport, 0),
                    new("Ryokan in Kyoto, 4 nights", 780.00m, ExpenseCategory.Accommodation, 0),
                    new("Tea ceremony in Gion", 45.00m, ExpenseCategory.Activities, 2),
                    new("Travel insurance", 38.00m, ExpenseCategory.Other, 0)
                },
                new SeedPackingItem[]
                {
                    new("Passport", 1, PackingCategory.Documents, false),
                    new("Cash in yen", 1, PackingCategory.Documents, false),
                    new("Pocket Wi-Fi reservation", 1, PackingCategory.Electronics, false),
                    new("Japan plug adapter", 1, PackingCategory.Electronics, false),
                    new("Camera", 1, PackingCategory.Electronics, false),
                    new("Light jacket", 1, PackingCategory.Clothing, false),
                    new("Slip-on shoes", 1, PackingCategory.Clothing, false),
                    new("Socks without holes", 5, PackingCategory.Clothing, false),
                    new("Allergy tablets", 1, PackingCategory.Health, false),
                    new("Small towel", 1, PackingCategory.Toiletries, false)
                }),
            new("ivan@voyage.test", "Barcelona Food Tour",
                "Tapas, markets and Gaudi architecture.",
                35, 5, 1300m,
                new[] { "Barcelona" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Barcelona", 212.00m, ExpenseCategory.Transport, 0),
                    new("Hostel in El Born, 5 nights", 310.00m, ExpenseCategory.Accommodation, 0),
                    new("Sagrada Familia with tower", 36.00m, ExpenseCategory.Activities, 1),
                    new("Tapas walking tour", 65.00m, ExpenseCategory.Food, 2)
                },
                new SeedPackingItem[]
                {
                    new("T-shirts", 4, PackingCategory.Clothing, false),
                    new("Shorts", 2, PackingCategory.Clothing, false),
                    new("Sneakers", 1, PackingCategory.Clothing, false),
                    new("Money belt", 1, PackingCategory.Other, false),
                    new("Phone charger", 1, PackingCategory.Electronics, false),
                    new("ID card", 1, PackingCategory.Documents, false),
                    new("Toothbrush", 1, PackingCategory.Toiletries, false)
                }),
            new("ivan@voyage.test", "Rila Lakes Hike",
                "A one-day hike around the Seven Rila Lakes.",
                -45, 1, 150m,
                new[] { "Rila Lakes" },
                new SeedExpense[]
                {
                    new("Fuel", 36.00m, ExpenseCategory.Transport, 0),
                    new("Chairlift Panichishte, return", 30.00m, ExpenseCategory.Activities, 0),
                    new("Lunch at Rilski Ezera hut", 18.50m, ExpenseCategory.Food, 0),
                    new("Snacks and water", 9.80m, ExpenseCategory.Food, 0)
                },
                new SeedPackingItem[]
                {
                    new("Hiking boots", 1, PackingCategory.Clothing, true),
                    new("Rain jacket", 1, PackingCategory.Clothing, true),
                    new("Warm hat", 1, PackingCategory.Clothing, true),
                    new("Water bottles", 2, PackingCategory.Other, true),
                    new("Sandwiches", 3, PackingCategory.Other, true),
                    new("First aid kit", 1, PackingCategory.Health, true),
                    new("Sunscreen", 1, PackingCategory.Toiletries, true)
                }),
            new("elena@voyage.test", "Iceland Road Trip",
                "Waterfalls, hot springs and northern lights.",
                80, 8, 3200m,
                new[] { "Reykjavik" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Reykjavik", 540.00m, ExpenseCategory.Transport, 0),
                    new("Car rental, 8 days", 720.00m, ExpenseCategory.Transport, 0),
                    new("Guesthouses along the Ring Road", 960.00m, ExpenseCategory.Accommodation, 0),
                    new("Sky Lagoon tickets", 89.00m, ExpenseCategory.Activities, 1)
                },
                new SeedPackingItem[]
                {
                    new("Waterproof jacket", 1, PackingCategory.Clothing, false),
                    new("Thermal layers", 3, PackingCategory.Clothing, false),
                    new("Hiking boots", 1, PackingCategory.Clothing, false),
                    new("Wool hat", 1, PackingCategory.Clothing, false),
                    new("Swimsuit", 1, PackingCategory.Clothing, false),
                    new("Driving licence", 1, PackingCategory.Documents, false),
                    new("Camera tripod", 1, PackingCategory.Electronics, false),
                    new("Car phone holder", 1, PackingCategory.Electronics, false),
                    new("Moisturiser", 1, PackingCategory.Toiletries, false)
                }),
            new("elena@voyage.test", "Istanbul Long Weekend",
                "Bazaars, mosques and a boat trip on the Bosphorus.",
                -20, 3, 700m,
                new[] { "Istanbul" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Istanbul", 168.00m, ExpenseCategory.Transport, 0),
                    new("Hotel in Sultanahmet, 3 nights", 234.00m, ExpenseCategory.Accommodation, 0),
                    new("Istanbulkart and metro", 15.00m, ExpenseCategory.Transport, 0),
                    new("Bosphorus cruise", 22.00m, ExpenseCategory.Activities, 1),
                    new("Topkapi Palace ticket", 38.00m, ExpenseCategory.Activities, 1),
                    new("Turkish breakfast and street food", 96.40m, ExpenseCategory.Food, 2),
                    new("Spices and tea from the bazaar", 41.50m, ExpenseCategory.Shopping, 2)
                },
                new SeedPackingItem[]
                {
                    new("Scarf", 1, PackingCategory.Clothing, true),
                    new("Comfortable shoes", 1, PackingCategory.Clothing, true),
                    new("Light cardigan", 1, PackingCategory.Clothing, true),
                    new("Passport", 1, PackingCategory.Documents, true),
                    new("Power bank", 1, PackingCategory.Electronics, true),
                    new("Hand sanitiser", 1, PackingCategory.Health, true)
                }),
            new("nikolay@voyage.test", "Canadian Rockies",
                "Turquoise lakes and mountain trails around Banff.",
                150, 10, 4800m,
                new[] { "Banff" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Calgary", 1240.00m, ExpenseCategory.Transport, 0),
                    new("eTA application", 7.00m, ExpenseCategory.Other, 0),
                    new("Lodge in Banff, 5 nights", 1150.00m, ExpenseCategory.Accommodation, 0)
                },
                new SeedPackingItem[]
                {
                    new("Passport", 1, PackingCategory.Documents, false),
                    new("eTA confirmation", 1, PackingCategory.Documents, false),
                    new("Bear spray", 1, PackingCategory.Other, false),
                    new("Hiking poles", 1, PackingCategory.Other, false),
                    new("Fleece jacket", 1, PackingCategory.Clothing, false),
                    new("Hiking boots", 1, PackingCategory.Clothing, false),
                    new("Insect repellent", 1, PackingCategory.Health, false),
                    new("Binoculars", 1, PackingCategory.Other, false)
                }),
            new("nikolay@voyage.test", "Northern Lights in Reykjavik",
                "Four winter nights chasing the aurora.",
                -70, 4, 1900m,
                new[] { "Reykjavik" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Reykjavik", 498.00m, ExpenseCategory.Transport, 0),
                    new("Apartment in the centre, 4 nights", 560.00m, ExpenseCategory.Accommodation, 0),
                    new("Northern lights tour", 95.00m, ExpenseCategory.Activities, 1),
                    new("Golden Circle tour", 110.00m, ExpenseCategory.Activities, 2),
                    new("Groceries", 84.30m, ExpenseCategory.Food, 1),
                    new("Fish and chips at the harbour", 31.00m, ExpenseCategory.Food, 3)
                },
                new SeedPackingItem[]
                {
                    new("Down jacket", 1, PackingCategory.Clothing, true),
                    new("Thermal layers", 2, PackingCategory.Clothing, true),
                    new("Snow boots", 1, PackingCategory.Clothing, true),
                    new("Camera", 1, PackingCategory.Electronics, true),
                    new("Tripod", 1, PackingCategory.Electronics, true),
                    new("Passport", 1, PackingCategory.Documents, true)
                }),
            new("petya@voyage.test", "Paris for Two",
                "Museums, cafes and an evening at the Eiffel Tower.",
                -15, 4, 1600m,
                new[] { "Paris" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Paris, 2 people", 412.00m, ExpenseCategory.Transport, 0),
                    new("Boutique hotel in Montmartre, 4 nights", 688.00m, ExpenseCategory.Accommodation, 0),
                    new("Louvre tickets", 44.00m, ExpenseCategory.Activities, 1),
                    new("Dinner cruise on the Seine", 198.00m, ExpenseCategory.Food, 2),
                    new("Cafes and bakeries", 126.80m, ExpenseCategory.Food, 3),
                    new("Perfume and souvenirs", 64.00m, ExpenseCategory.Shopping, 3)
                },
                new SeedPackingItem[]
                {
                    new("Evening dress", 1, PackingCategory.Clothing, true),
                    new("Ballet flats", 1, PackingCategory.Clothing, true),
                    new("Trench coat", 1, PackingCategory.Clothing, true),
                    new("Umbrella", 1, PackingCategory.Other, true),
                    new("Camera", 1, PackingCategory.Electronics, true),
                    new("Makeup bag", 1, PackingCategory.Toiletries, true),
                    new("Passport", 1, PackingCategory.Documents, true)
                }),
            new("petya@voyage.test", "Bali Retreat",
                "Rice terraces, temples and quiet beaches.",
                200, 14, 3900m,
                new[] { "Bali" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Denpasar", 1050.00m, ExpenseCategory.Transport, 0),
                    new("Villa in Ubud, 7 nights", 640.00m, ExpenseCategory.Accommodation, 0),
                    new("Yoga retreat, 5 days", 420.00m, ExpenseCategory.Activities, 7)
                },
                new SeedPackingItem[]
                {
                    new("Light dresses", 5, PackingCategory.Clothing, false),
                    new("Swimsuit", 2, PackingCategory.Clothing, false),
                    new("Mosquito repellent", 2, PackingCategory.Health, false),
                    new("Probiotics", 1, PackingCategory.Health, false),
                    new("Reef-safe sunscreen", 2, PackingCategory.Toiletries, false),
                    new("Yoga mat", 1, PackingCategory.Other, false),
                    new("Passport", 1, PackingCategory.Documents, false),
                    new("Universal adapter", 1, PackingCategory.Electronics, false)
                }),
            new("stefan@voyage.test", "Velingrad Spa Weekend",
                "Mineral pools and pine forests in the Rhodopes.",
                10, 2, 300m,
                new[] { "Velingrad" },
                new SeedExpense[]
                {
                    new("Spa hotel, 2 nights with breakfast", 210.00m, ExpenseCategory.Accommodation, 0),
                    new("Fuel", 32.00m, ExpenseCategory.Transport, 0)
                },
                new SeedPackingItem[]
                {
                    new("Swimsuit", 1, PackingCategory.Clothing, false),
                    new("Bathrobe", 1, PackingCategory.Clothing, false),
                    new("Slippers", 1, PackingCategory.Clothing, true),
                    new("Book", 1, PackingCategory.Other, true),
                    new("Shower gel", 1, PackingCategory.Toiletries, false)
                }),
            new("stefan@voyage.test", "Christmas Markets in Prague",
                "Mulled wine, lights and the Old Town square in December.",
                -340, 3, 800m,
                new[] { "Prague" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Prague", 142.00m, ExpenseCategory.Transport, 0),
                    new("Hotel near Old Town Square, 3 nights", 288.00m, ExpenseCategory.Accommodation, 0),
                    new("Mulled wine and trdelnik", 27.00m, ExpenseCategory.Food, 1),
                    new("Christmas ornaments", 34.50m, ExpenseCategory.Shopping, 2),
                    new("Dinner at a brewery", 48.00m, ExpenseCategory.Food, 2)
                },
                new SeedPackingItem[]
                {
                    new("Winter coat", 1, PackingCategory.Clothing, true),
                    new("Gloves", 1, PackingCategory.Clothing, true),
                    new("Warm boots", 1, PackingCategory.Clothing, true),
                    new("ID card", 1, PackingCategory.Documents, true),
                    new("Phone charger", 1, PackingCategory.Electronics, true)
                }),
            new("ana@voyage.test", "Croatian Lakes",
                "Wooden paths and waterfalls in Plitvice.",
                95, 4, 900m,
                new[] { "Plitvice Lakes" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Zagreb", 146.00m, ExpenseCategory.Transport, 0),
                    new("Rental car, 4 days", 168.00m, ExpenseCategory.Transport, 0),
                    new("Guest house near the park, 3 nights", 195.00m, ExpenseCategory.Accommodation, 0)
                },
                new SeedPackingItem[]
                {
                    new("Hiking shoes", 1, PackingCategory.Clothing, false),
                    new("Light rain jacket", 1, PackingCategory.Clothing, false),
                    new("Camera", 1, PackingCategory.Electronics, false),
                    new("ID card", 1, PackingCategory.Documents, false),
                    new("Water bottle", 1, PackingCategory.Other, false),
                    new("Sunscreen", 1, PackingCategory.Toiletries, false)
                }),
            new("ana@voyage.test", "Istanbul Food Week",
                "Cooking classes, markets and long dinners by the Bosphorus.",
                -2, 5, 1100m,
                new[] { "Istanbul" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Istanbul", 176.00m, ExpenseCategory.Transport, 0),
                    new("Apartment in Karakoy, 5 nights", 390.00m, ExpenseCategory.Accommodation, 0),
                    new("Turkish cooking class", 65.00m, ExpenseCategory.Activities, 1),
                    new("Kadikoy market and street food", 38.20m, ExpenseCategory.Food, 1)
                },
                new SeedPackingItem[]
                {
                    new("Comfortable shoes", 1, PackingCategory.Clothing, true),
                    new("Scarf", 1, PackingCategory.Clothing, true),
                    new("Reusable shopping bag", 1, PackingCategory.Other, true),
                    new("Passport", 1, PackingCategory.Documents, true),
                    new("Antacid tablets", 1, PackingCategory.Health, true),
                    new("Power bank", 1, PackingCategory.Electronics, false)
                }),
            new("dimitar@voyage.test", "Ski Week in Borovets",
                "A week on the slopes with friends.",
                40, 6, 800m,
                new[] { "Borovets" },
                new SeedExpense[]
                {
                    new("Apartment share, 6 nights", 240.00m, ExpenseCategory.Accommodation, 0),
                    new("Lift pass, 6 days", 312.00m, ExpenseCategory.Activities, 0),
                    new("Fuel and parking", 45.00m, ExpenseCategory.Transport, 0)
                },
                new SeedPackingItem[]
                {
                    new("Ski jacket", 1, PackingCategory.Clothing, false),
                    new("Ski goggles", 1, PackingCategory.Other, false),
                    new("Thermal underwear", 2, PackingCategory.Clothing, false),
                    new("Helmet", 1, PackingCategory.Other, false),
                    new("Board games", 1, PackingCategory.Other, false),
                    new("Lip balm", 1, PackingCategory.Toiletries, false)
                }),
            new("dimitar@voyage.test", "New Zealand Adventure",
                "Bungee jumping, fjords and mountain views.",
                -120, 14, 5200m,
                new[] { "Queenstown" },
                new SeedExpense[]
                {
                    new("Flights Sofia - Queenstown", 1890.00m, ExpenseCategory.Transport, 0),
                    new("Campervan rental, 14 days", 1340.00m, ExpenseCategory.Transport, 0),
                    new("Kawarau bungee jump", 265.00m, ExpenseCategory.Activities, 2),
                    new("Milford Sound cruise", 145.00m, ExpenseCategory.Activities, 5),
                    new("Fergburger and pies", 42.00m, ExpenseCategory.Food, 1),
                    new("Groceries and campsites", 412.00m, ExpenseCategory.Food, 13),
                    new("Merino wool jumper", 120.00m, ExpenseCategory.Shopping, 9)
                },
                new SeedPackingItem[]
                {
                    new("Hiking boots", 1, PackingCategory.Clothing, true),
                    new("Rain jacket", 1, PackingCategory.Clothing, true),
                    new("Fleece", 1, PackingCategory.Clothing, true),
                    new("Passport", 1, PackingCategory.Documents, true),
                    new("International driving permit", 1, PackingCategory.Documents, true),
                    new("Camera", 1, PackingCategory.Electronics, true),
                    new("Universal adapter", 1, PackingCategory.Electronics, true),
                    new("Sandfly repellent", 1, PackingCategory.Health, true)
                })
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

            foreach (var ownerTrips in Trips.GroupBy(t => t.OwnerEmail))
            {
                var owner = await userManager.FindByEmailAsync(ownerTrips.Key);

                if (owner == null || await dbContext.Trips.AnyAsync(t => t.OwnerId == owner.Id))
                {
                    continue;
                }

                foreach (var seedTrip in ownerTrips)
                {
                    dbContext.Trips.Add(CreateTrip(seedTrip, owner.Id, destinationIds));
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static Trip CreateTrip(SeedTrip seedTrip, string ownerId, Dictionary<string, int> destinationIds)
        {
            var startDate = DateTime.Today.AddDays(seedTrip.StartInDays);
            var endDate = startDate.AddDays(seedTrip.LengthInDays);

            var trip = new Trip
            {
                Title = seedTrip.Title,
                Description = seedTrip.Description,
                StartDate = startDate,
                EndDate = endDate,
                Budget = seedTrip.Budget,
                CreatedOn = DateTime.UtcNow,
                OwnerId = ownerId
            };

            foreach (var destinationName in seedTrip.Destinations)
            {
                if (destinationIds.TryGetValue(destinationName, out var destinationId))
                {
                    trip.TripDestinations.Add(new TripDestination { DestinationId = destinationId });
                }
            }

            foreach (var expense in seedTrip.Expenses)
            {
                trip.Expenses.Add(new Expense
                {
                    Title = expense.Title,
                    Amount = expense.Amount,
                    Category = expense.Category,
                    Date = startDate.AddDays(expense.DayOfTrip)
                });
            }

            foreach (var item in seedTrip.PackingItems)
            {
                trip.PackingItems.Add(new PackingItem
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Category = item.Category,
                    IsPacked = item.IsPacked,
                    CreatedOn = DateTime.UtcNow
                });
            }

            return trip;
        }

        private sealed record SeedTrip(
            string OwnerEmail,
            string Title,
            string Description,
            int StartInDays,
            int LengthInDays,
            decimal Budget,
            string[] Destinations,
            SeedExpense[] Expenses,
            SeedPackingItem[] PackingItems);

        private sealed record SeedExpense(
            string Title,
            decimal Amount,
            ExpenseCategory Category,
            int DayOfTrip);

        private sealed record SeedPackingItem(
            string Name,
            int Quantity,
            PackingCategory Category,
            bool IsPacked);
    }
}
