using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Voyage.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoriesAndDestinations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Sun, sand and sea escapes.", "Beach" },
                    { 2, "Peaks, trails and fresh air.", "Mountains" },
                    { 3, "Short trips to vibrant cities.", "City Break" },
                    { 4, "History, museums and heritage.", "Culture" },
                    { 5, "Parks, lakes and wildlife.", "Nature" },
                    { 6, "Thrills and outdoor activities.", "Adventure" },
                    { 7, "Spa retreats and relaxing getaways.", "Wellness" },
                    { 8, "Snowy escapes and ski resorts.", "Winter" }
                });

            migrationBuilder.InsertData(
                table: "Destinations",
                columns: new[] { "Id", "CategoryId", "Country", "Description", "ImageUrl", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Greece", "Whitewashed villages, blue domes and famous sunsets over the Aegean Sea.", "/images/destinations/santorini.jpg", false, "Santorini" },
                    { 2, 2, "Bulgaria", "Mountain town at the foot of Pirin, popular for skiing and hiking.", "/images/destinations/bansko.jpg", false, "Bansko" },
                    { 3, 3, "France", "The city of light with iconic landmarks, cafés and world-class museums.", "/images/destinations/paris.jpg", false, "Paris" },
                    { 4, 4, "Italy", "Ancient ruins, Renaissance art and lively piazzas in the Eternal City.", "/images/destinations/rome.jpg", false, "Rome" },
                    { 5, 5, "Croatia", "Terraced lakes connected by waterfalls in a protected national park.", "/images/destinations/plitvice.jpg", false, "Plitvice Lakes" },
                    { 6, 6, "Switzerland", "Base for paragliding, canyoning and alpine adventures between two lakes.", "/images/destinations/interlaken.jpg", false, "Interlaken" },
                    { 7, 1, "Bulgaria", "Charming old town on the Black Sea coast with sandy beaches.", "/images/destinations/sozopol.jpg", false, "Sozopol" },
                    { 8, 3, "Spain", "Gaudí architecture, beaches and vibrant nightlife on the Mediterranean.", "/images/destinations/barcelona.jpg", false, "Barcelona" },
                    { 9, 3, "Czech Republic", "Fairytale old town with a famous astronomical clock and Charles Bridge.", "/images/destinations/prague.jpg", false, "Prague" },
                    { 10, 4, "Japan", "Thousands of temples, traditional tea houses and beautiful gardens.", "/images/destinations/kyoto.jpg", false, "Kyoto" },
                    { 11, 5, "Canada", "Turquoise lakes and rugged peaks in the heart of the Rockies.", "/images/destinations/banff.jpg", false, "Banff" },
                    { 12, 6, "New Zealand", "The adventure capital of the world for bungee jumping and skiing.", "/images/destinations/queenstown.jpg", false, "Queenstown" },
                    { 13, 1, "Indonesia", "Tropical beaches, rice terraces and spiritual temples.", "/images/destinations/bali.jpg", false, "Bali" },
                    { 14, 8, "Switzerland", "Car-free alpine village beneath the iconic Matterhorn.", "/images/destinations/zermatt.jpg", false, "Zermatt" },
                    { 15, 7, "Bulgaria", "The spa capital of the Balkans with dozens of mineral springs.", "/images/destinations/velingrad.jpg", false, "Velingrad" },
                    { 16, 4, "Turkey", "Where Europe meets Asia, full of bazaars, mosques and palaces.", "/images/destinations/istanbul.jpg", false, "Istanbul" },
                    { 17, 2, "Bulgaria", "Seven glacial lakes high in the Rila Mountains.", "/images/destinations/rila.jpg", false, "Rila Lakes" },
                    { 18, 5, "Iceland", "Gateway to geysers, waterfalls and the northern lights.", "/images/destinations/reykjavik.jpg", false, "Reykjavik" },
                    { 19, 7, "Hungary", "Grand thermal baths and elegant architecture along the Danube.", "/images/destinations/budapest.jpg", false, "Budapest" },
                    { 20, 8, "Bulgaria", "Bulgaria’s oldest ski resort surrounded by pine forests.", "/images/destinations/borovets.jpg", false, "Borovets" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
