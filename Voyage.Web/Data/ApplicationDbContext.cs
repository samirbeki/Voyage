using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Destination> Destinations { get; set; } = null!;

        public DbSet<Trip> Trips { get; set; } = null!;

        public DbSet<TripDestination> TripDestinations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Beach",
                    Description = "Sun, sand and sea escapes."
                },
                new Category
                {
                    Id = 2,
                    Name = "Mountains",
                    Description = "Peaks, trails and fresh air."
                },
                new Category
                {
                    Id = 3,
                    Name = "City Break",
                    Description = "Short trips to vibrant cities."
                },
                new Category
                {
                    Id = 4,
                    Name = "Culture",
                    Description = "History, museums and heritage."
                },
                new Category
                {
                    Id = 5,
                    Name = "Nature",
                    Description = "Parks, lakes and wildlife."
                },
                new Category
                {
                    Id = 6,
                    Name = "Adventure",
                    Description = "Thrills and outdoor activities."
                },
                new Category
                {
                    Id = 7,
                    Name = "Wellness",
                    Description = "Spa retreats and relaxing getaways."
                },
                new Category
                {
                    Id = 8,
                    Name = "Winter",
                    Description = "Snowy escapes and ski resorts."
                }
            );

            builder.Entity<Destination>().HasData(
                new Destination
                {
                    Id = 1,
                    Name = "Santorini",
                    Country = "Greece",
                    CategoryId = 1,
                    ImageUrl = "/images/destinations/santorini.jpg",
                    Description = "Whitewashed villages, blue domes and famous sunsets over the Aegean Sea."
                },
                new Destination
                {
                    Id = 2,
                    Name = "Bansko",
                    Country = "Bulgaria",
                    CategoryId = 2,
                    ImageUrl = "/images/destinations/bansko.jpg",
                    Description = "Mountain town at the foot of Pirin, popular for skiing and hiking."
                },
                new Destination
                {
                    Id = 3,
                    Name = "Paris",
                    Country = "France",
                    CategoryId = 3,
                    ImageUrl = "/images/destinations/paris.jpg",
                    Description = "The city of light with iconic landmarks, cafés and world-class museums."
                },
                new Destination
                {
                    Id = 4,
                    Name = "Rome",
                    Country = "Italy",
                    CategoryId = 4,
                    ImageUrl = "/images/destinations/rome.jpg",
                    Description = "Ancient ruins, Renaissance art and lively piazzas in the Eternal City."
                },
                new Destination
                {
                    Id = 5,
                    Name = "Plitvice Lakes",
                    Country = "Croatia",
                    CategoryId = 5,
                    ImageUrl = "/images/destinations/plitvice.jpg",
                    Description = "Terraced lakes connected by waterfalls in a protected national park."
                },
                new Destination
                {
                    Id = 6,
                    Name = "Interlaken",
                    Country = "Switzerland",
                    CategoryId = 6,
                    ImageUrl = "/images/destinations/interlaken.jpg",
                    Description = "Base for paragliding, canyoning and alpine adventures between two lakes."
                },
                new Destination
                {
                    Id = 7,
                    Name = "Sozopol",
                    Country = "Bulgaria",
                    CategoryId = 1,
                    ImageUrl = "/images/destinations/sozopol.jpg",
                    Description = "Charming old town on the Black Sea coast with sandy beaches."
                },
                new Destination
                {
                    Id = 8,
                    Name = "Barcelona",
                    Country = "Spain",
                    CategoryId = 3,
                    ImageUrl = "/images/destinations/barcelona.jpg",
                    Description = "Gaudí architecture, beaches and vibrant nightlife on the Mediterranean."
                },
                new Destination
                {
                    Id = 9,
                    Name = "Prague",
                    Country = "Czech Republic",
                    CategoryId = 3,
                    ImageUrl = "/images/destinations/prague.jpg",
                    Description = "Fairytale old town with a famous astronomical clock and Charles Bridge."
                },
                new Destination
                {
                    Id = 10,
                    Name = "Kyoto",
                    Country = "Japan",
                    CategoryId = 4,
                    ImageUrl = "/images/destinations/kyoto.jpg",
                    Description = "Thousands of temples, traditional tea houses and beautiful gardens."
                },
                new Destination
                {
                    Id = 11,
                    Name = "Banff",
                    Country = "Canada",
                    CategoryId = 5,
                    ImageUrl = "/images/destinations/banff.jpg",
                    Description = "Turquoise lakes and rugged peaks in the heart of the Rockies."
                },
                new Destination
                {
                    Id = 12,
                    Name = "Queenstown",
                    Country = "New Zealand",
                    CategoryId = 6,
                    ImageUrl = "/images/destinations/queenstown.jpg",
                    Description = "The adventure capital of the world for bungee jumping and skiing."
                },
                new Destination
                {
                    Id = 13,
                    Name = "Bali",
                    Country = "Indonesia",
                    CategoryId = 1,
                    ImageUrl = "/images/destinations/bali.jpg",
                    Description = "Tropical beaches, rice terraces and spiritual temples."
                },
                new Destination
                {
                    Id = 14,
                    Name = "Zermatt",
                    Country = "Switzerland",
                    CategoryId = 8,
                    ImageUrl = "/images/destinations/zermatt.jpg",
                    Description = "Car-free alpine village beneath the iconic Matterhorn."
                },
                new Destination
                {
                    Id = 15,
                    Name = "Velingrad",
                    Country = "Bulgaria",
                    CategoryId = 7,
                    ImageUrl = "/images/destinations/velingrad.jpg",
                    Description = "The spa capital of the Balkans with dozens of mineral springs."
                },
                new Destination
                {
                    Id = 16,
                    Name = "Istanbul",
                    Country = "Turkey",
                    CategoryId = 4,
                    ImageUrl = "/images/destinations/istanbul.jpg",
                    Description = "Where Europe meets Asia, full of bazaars, mosques and palaces."
                },
                new Destination
                {
                    Id = 17,
                    Name = "Rila Lakes",
                    Country = "Bulgaria",
                    CategoryId = 2,
                    ImageUrl = "/images/destinations/rila.jpg",
                    Description = "Seven glacial lakes high in the Rila Mountains."
                },
                new Destination
                {
                    Id = 18,
                    Name = "Reykjavik",
                    Country = "Iceland",
                    CategoryId = 5,
                    ImageUrl = "/images/destinations/reykjavik.jpg",
                    Description = "Gateway to geysers, waterfalls and the northern lights."
                },
                new Destination
                {
                    Id = 19,
                    Name = "Budapest",
                    Country = "Hungary",
                    CategoryId = 7,
                    ImageUrl = "/images/destinations/budapest.jpg",
                    Description = "Grand thermal baths and elegant architecture along the Danube."
                },
                new Destination
                {
                    Id = 20,
                    Name = "Borovets",
                    Country = "Bulgaria",
                    CategoryId = 8,
                    ImageUrl = "/images/destinations/borovets.jpg",
                    Description = "Bulgaria’s oldest ski resort surrounded by pine forests."
                }
            );
        }
    }
}