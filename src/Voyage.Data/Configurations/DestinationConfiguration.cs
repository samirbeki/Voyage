using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Data.Models;

namespace Voyage.Data.Configurations
{
    public class DestinationConfiguration : IEntityTypeConfiguration<Destination>
    {
        public void Configure(EntityTypeBuilder<Destination> builder)
        {
            builder.HasIndex(d => d.CategoryId);

            builder.HasOne(d => d.Category)
                .WithMany(c => c.Destinations)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(d => !d.IsDeleted);

            builder.HasData(
                new Destination
                {
                    Id = 1,
                    Name = "Santorini",
                    Country = "Greece",
                    CategoryId = 1,
                    Latitude = 36.4166,
                    Longitude = 25.4318,
                    ImageUrl = "/images/destinations/santorini.jpg",
                    Description = Paragraphs(
                        "Whitewashed villages, blue domes and famous sunsets over the Aegean Sea make Santorini one of the most photographed islands in Greece.",
                        "The island is the rim of a volcanic caldera, so towns like Fira and Oia sit on cliffs high above the water. Walk the path between them, take a boat to the volcano and its hot springs, or visit Akrotiri, a Bronze Age town preserved under volcanic ash.",
                        "The beaches here are black or red sand, and local wineries grow Assyrtiko vines in a unique basket shape. Late spring and early autumn are the best time to visit, when it is warm but far less crowded than in August.")
                },
                new Destination
                {
                    Id = 2,
                    Name = "Bansko",
                    Country = "Bulgaria",
                    CategoryId = 2,
                    Latitude = 41.8383,
                    Longitude = 23.4885,
                    ImageUrl = "/images/destinations/bansko.jpg",
                    Description = Paragraphs(
                        "A mountain town at the foot of Pirin, Bansko is the most popular winter resort in Bulgaria and a great base for summer hiking.",
                        "In winter a gondola takes skiers from the edge of town up to the slopes below Todorka peak, with runs for every level. In summer the same mountains open trails to Vihren, the highest peak in Pirin, and to glacial lakes such as Popovo.",
                        "The old town keeps its stone houses, cobbled streets and the Holy Trinity church. In the evening, look for a traditional mehana with live folk music and try kapama, a slow-cooked dish of meat, rice and sauerkraut.")
                },
                new Destination
                {
                    Id = 3,
                    Name = "Paris",
                    Country = "France",
                    CategoryId = 3,
                    Latitude = 48.8566,
                    Longitude = 2.3522,
                    ImageUrl = "/images/destinations/paris.jpg",
                    Description = Paragraphs(
                        "The city of light, Paris is full of iconic landmarks, sidewalk cafés and some of the best museums in the world.",
                        "Climb the Eiffel Tower, see the Mona Lisa at the Louvre and the Impressionists at the Musée d'Orsay, then walk along the Seine to Notre-Dame and the Latin Quarter. Montmartre and the Sacré-Cœur offer one of the best views over the rooftops.",
                        "Paris is best explored on foot and by metro. Book the popular museums in advance, buy fresh bread from a local boulangerie and leave an evening for a boat ride on the river when the monuments are lit up.")
                },
                new Destination
                {
                    Id = 4,
                    Name = "Rome",
                    Country = "Italy",
                    CategoryId = 4,
                    Latitude = 41.9028,
                    Longitude = 12.4964,
                    ImageUrl = "/images/destinations/rome.jpg",
                    Description = Paragraphs(
                        "Ancient ruins, Renaissance art and lively piazzas fill the Eternal City, where almost every street holds a piece of history.",
                        "Start with the Colosseum, the Roman Forum and the Palatine Hill, then see the Pantheon and throw a coin into the Trevi Fountain. In Vatican City, the Vatican Museums lead to the Sistine Chapel, and the dome of St. Peter's Basilica gives a view over the whole city.",
                        "Trastevere is the place for an evening walk and a plate of cacio e pepe or carbonara. Spring and autumn are the most comfortable seasons, as summers in Rome are hot and very busy.")
                },
                new Destination
                {
                    Id = 5,
                    Name = "Plitvice Lakes",
                    Country = "Croatia",
                    CategoryId = 5,
                    Latitude = 44.8654,
                    Longitude = 15.5820,
                    ImageUrl = "/images/destinations/plitvice.jpg",
                    Description = Paragraphs(
                        "Terraced lakes connected by waterfalls make Plitvice Lakes National Park one of the most beautiful natural sights in Europe.",
                        "Sixteen named lakes step down through a forested valley, and their water changes from turquoise to green depending on the minerals and the light. Wooden boardwalks lead across the water and past Veliki Slap, the highest waterfall in Croatia at about 78 metres.",
                        "The park is a UNESCO World Heritage Site and swimming is not allowed. Arrive early in the morning or come outside July and August to enjoy the trails with fewer people, and plan at least half a day for the main routes.")
                },
                new Destination
                {
                    Id = 6,
                    Name = "Interlaken",
                    Country = "Switzerland",
                    CategoryId = 6,
                    Latitude = 46.6863,
                    Longitude = 7.8632,
                    ImageUrl = "/images/destinations/interlaken.jpg",
                    Description = Paragraphs(
                        "Set between Lake Thun and Lake Brienz, Interlaken is the base for paragliding, canyoning and alpine adventures in the Bernese Oberland.",
                        "From the town, trains and cable cars climb toward the famous trio of Eiger, Mönch and Jungfrau. The Jungfraujoch, often called the Top of Europe, has the highest railway station in Europe at 3,454 metres.",
                        "Adventure seekers can paraglide over the town, raft the rivers or try canyoning, while calmer days are perfect for boat trips on the lakes or a ride up to Harder Kulm for the view. Regional travel passes make exploring the valleys much cheaper.")
                },
                new Destination
                {
                    Id = 7,
                    Name = "Sozopol",
                    Country = "Bulgaria",
                    CategoryId = 1,
                    Latitude = 42.4178,
                    Longitude = 27.6956,
                    ImageUrl = "/images/destinations/sozopol.jpg",
                    Description = Paragraphs(
                        "A charming old town on the Black Sea coast with sandy beaches, Sozopol is one of the oldest towns in Bulgaria.",
                        "Founded by Greek colonists as Apollonia, the town sits on a small peninsula with narrow cobbled streets and wooden houses from the Bulgarian Revival. Walk along the restored fortress walls, visit the archaeological museum and watch the fishing boats return to the harbour.",
                        "The Central and Harmanite beaches are close to the town, while quieter bays lie to the south. At the end of every summer Sozopol hosts the Apollonia arts festival with concerts, theatre and exhibitions.")
                },
                new Destination
                {
                    Id = 8,
                    Name = "Barcelona",
                    Country = "Spain",
                    CategoryId = 3,
                    Latitude = 41.3874,
                    Longitude = 2.1686,
                    ImageUrl = "/images/destinations/barcelona.jpg",
                    Description = Paragraphs(
                        "Gaudí architecture, city beaches and vibrant nightlife come together in Barcelona, the capital of Catalonia on the Mediterranean coast.",
                        "The unfinished Sagrada Família, Park Güell and Casa Batlló show Gaudí at his most creative. The narrow lanes of the Gothic Quarter lead to La Rambla and the Boqueria market, full of fresh fruit, seafood and tapas bars.",
                        "Barceloneta beach is a short walk from the old town, and Montjuïc hill offers gardens, museums and views over the port. Dinner here starts late, often after nine in the evening, so take it slow and enjoy the rhythm of the city.")
                },
                new Destination
                {
                    Id = 9,
                    Name = "Prague",
                    Country = "Czech Republic",
                    CategoryId = 3,
                    Latitude = 50.0755,
                    Longitude = 14.4378,
                    ImageUrl = "/images/destinations/prague.jpg",
                    Description = Paragraphs(
                        "A fairytale old town with a famous astronomical clock and the Charles Bridge, Prague is one of the best-preserved capitals in Europe.",
                        "Watch the clock on the Old Town Hall strike the hour, cross the Charles Bridge early in the morning and climb to Prague Castle, one of the largest castle complexes in the world. Below the castle, Malá Strana is full of baroque palaces and quiet gardens.",
                        "Czech cuisine is hearty, so try svíčková or goulash with dumplings and a glass of local beer. Prague is compact and easy to explore on foot, and trams reach almost every part of the city.")
                },
                new Destination
                {
                    Id = 10,
                    Name = "Kyoto",
                    Country = "Japan",
                    CategoryId = 4,
                    Latitude = 35.0116,
                    Longitude = 135.7681,
                    ImageUrl = "/images/destinations/kyoto.jpg",
                    Description = Paragraphs(
                        "Over a thousand temples, traditional tea houses and beautiful gardens make Kyoto the cultural heart of Japan.",
                        "For more than a thousand years Kyoto was the imperial capital. Visit the golden pavilion of Kinkaku-ji, walk under the thousands of red torii gates at Fushimi Inari and stroll through the bamboo grove in Arashiyama.",
                        "In the evening, the old streets of Gion are the best place to spot a geiko or maiko. Cherry blossoms in early April and red maple leaves in November are the most beautiful seasons, but also the busiest.")
                },
                new Destination
                {
                    Id = 11,
                    Name = "Banff",
                    Country = "Canada",
                    CategoryId = 5,
                    Latitude = 51.1784,
                    Longitude = -115.5708,
                    ImageUrl = "/images/destinations/banff.jpg",
                    Description = Paragraphs(
                        "Turquoise lakes and rugged peaks surround Banff, a small mountain town in the heart of the Canadian Rockies.",
                        "Banff National Park is the oldest national park in Canada, founded in 1885. Lake Louise and Moraine Lake glow bright blue in summer thanks to glacial rock flour, and the Icefields Parkway is one of the most scenic drives in the world.",
                        "Ride the gondola up Sulphur Mountain, soak in the Banff Upper Hot Springs and keep an eye out for elk, bighorn sheep and bears. Winter brings skiing at three nearby resorts and frozen lakes for skating.")
                },
                new Destination
                {
                    Id = 12,
                    Name = "Queenstown",
                    Country = "New Zealand",
                    CategoryId = 6,
                    Latitude = -45.0312,
                    Longitude = 168.6626,
                    ImageUrl = "/images/destinations/queenstown.jpg",
                    Description = Paragraphs(
                        "On the shores of Lake Wakatipu, Queenstown is known as the adventure capital of the world for bungee jumping, jet boating and skiing.",
                        "Commercial bungee jumping started here at the Kawarau Bridge in 1988, and you can still jump from it today. Jet boats race through the canyons of the Shotover River, and the Skyline gondola leads to views over the lake and the Remarkables range.",
                        "Queenstown is also the gateway to Milford Sound in Fiordland National Park. In winter, Coronet Peak and The Remarkables become busy ski areas, while summer is perfect for hiking and wine tasting in the nearby Gibbston Valley.")
                },
                new Destination
                {
                    Id = 13,
                    Name = "Bali",
                    Country = "Indonesia",
                    CategoryId = 1,
                    Latitude = -8.4095,
                    Longitude = 115.1889,
                    ImageUrl = "/images/destinations/bali.jpg",
                    Description = Paragraphs(
                        "Tropical beaches, green rice terraces and spiritual temples make Bali one of the most popular islands in Indonesia.",
                        "Ubud is the cultural centre of the island, surrounded by the Tegallalang rice terraces and home to the Sacred Monkey Forest. On the coast, Tanah Lot and Uluwatu are temples built on rocks above the ocean, with unforgettable sunsets.",
                        "Surfers head to Kuta and Canggu, while divers explore the reefs around Amed and Nusa Penida. The dry season from April to October is the best time to visit, and modest clothing is required when entering temples.")
                },
                new Destination
                {
                    Id = 14,
                    Name = "Zermatt",
                    Country = "Switzerland",
                    CategoryId = 8,
                    Latitude = 46.0207,
                    Longitude = 7.7491,
                    ImageUrl = "/images/destinations/zermatt.jpg",
                    Description = Paragraphs(
                        "A car-free alpine village beneath the iconic Matterhorn, Zermatt is one of the most famous mountain resorts in Switzerland.",
                        "Visitors arrive by train and move around the village on foot or in small electric taxis. The Gornergrat railway and the Matterhorn Glacier Paradise cable car reach viewpoints above 3,000 metres with panoramas of dozens of four-thousand-metre peaks.",
                        "Thanks to the glacier, skiing is possible all year round. In summer, the Five Lakes Walk offers some of the best reflections of the Matterhorn, and the village is full of cosy chalets and mountain restaurants.")
                },
                new Destination
                {
                    Id = 15,
                    Name = "Velingrad",
                    Country = "Bulgaria",
                    CategoryId = 7,
                    Latitude = 42.0270,
                    Longitude = 23.9910,
                    ImageUrl = "/images/destinations/velingrad.jpg",
                    Description = Paragraphs(
                        "Known as the spa capital of the Balkans, Velingrad has dozens of mineral springs in a green valley of the Western Rhodopes.",
                        "The town has more than seventy mineral springs, and many hotels offer pools and treatments with warm mineral water. Kleptuza, a large karst spring in the town, feeds small lakes surrounded by a park where you can rent a boat.",
                        "The pine forests around Velingrad are great for walks and cycling, and the narrow-gauge Rhodope railway passes through the town on its scenic route between Septemvri and Dobrinishte.")
                },
                new Destination
                {
                    Id = 16,
                    Name = "Istanbul",
                    Country = "Turkey",
                    CategoryId = 4,
                    Latitude = 41.0082,
                    Longitude = 28.9784,
                    ImageUrl = "/images/destinations/istanbul.jpg",
                    Description = Paragraphs(
                        "Where Europe meets Asia, Istanbul is a city full of bazaars, mosques and palaces on both shores of the Bosphorus.",
                        "In the historic Sultanahmet district, Hagia Sophia, the Blue Mosque and the underground Basilica Cistern stand only minutes apart. Topkapı Palace was the home of the Ottoman sultans for centuries, and the Grand Bazaar is one of the largest covered markets in the world.",
                        "Take a public ferry across the Bosphorus to the Asian side, try a fresh simit with Turkish tea, and end the day in Galata with a view over the Golden Horn.")
                },
                new Destination
                {
                    Id = 17,
                    Name = "Rila Lakes",
                    Country = "Bulgaria",
                    CategoryId = 2,
                    Latitude = 42.2040,
                    Longitude = 23.3150,
                    ImageUrl = "/images/destinations/rila.jpg",
                    Description = Paragraphs(
                        "Seven glacial lakes lie one above the other high in the Rila Mountains, making them one of the most visited natural sights in Bulgaria.",
                        "The lakes sit between about 2,100 and 2,500 metres above sea level, and each has a name that describes its shape, such as the Eye, the Kidney and the Tear. A chairlift from the Panichishte area brings visitors close to the Rila Lakes hut, where the trail begins.",
                        "The walk around the lakes takes about three to four hours, and the best season is from July to September, when the snow has melted. Bring warm layers, as the weather in the mountains changes quickly even in summer.")
                },
                new Destination
                {
                    Id = 18,
                    Name = "Reykjavik",
                    Country = "Iceland",
                    CategoryId = 5,
                    Latitude = 64.1466,
                    Longitude = -21.9426,
                    ImageUrl = "/images/destinations/reykjavik.jpg",
                    Description = Paragraphs(
                        "The northernmost capital of any country in the world, Reykjavik is the gateway to geysers, waterfalls and the northern lights.",
                        "The Golden Circle route links Þingvellir National Park, the Geysir geothermal area and the Gullfoss waterfall in a single day trip. Closer to the city, the Blue Lagoon and many local pools let you relax in warm geothermal water.",
                        "In the city itself, visit the Hallgrímskirkja church and the Harpa concert hall by the harbour. From September to March the long nights give the best chance to see the northern lights, while summer brings almost endless daylight.")
                },
                new Destination
                {
                    Id = 19,
                    Name = "Budapest",
                    Country = "Hungary",
                    CategoryId = 7,
                    Latitude = 47.4979,
                    Longitude = 19.0402,
                    ImageUrl = "/images/destinations/budapest.jpg",
                    Description = Paragraphs(
                        "Grand thermal baths and elegant architecture line the Danube in Budapest, the capital of Hungary.",
                        "The city is made of two parts: hilly Buda with the Castle District and the Fisherman's Bastion, and flat Pest with the Parliament building and lively streets. The Széchenyi and Rudas baths are perfect for an afternoon in warm thermal water.",
                        "In the evening, take a cruise on the Danube to see the bridges and the Parliament lit up, or explore the ruin bars in the old Jewish Quarter. Try a bowl of goulash soup and a chimney cake from a street stand.")
                },
                new Destination
                {
                    Id = 20,
                    Name = "Borovets",
                    Country = "Bulgaria",
                    CategoryId = 8,
                    Latitude = 42.2667,
                    Longitude = 23.6000,
                    ImageUrl = "/images/destinations/borovets.jpg",
                    Description = Paragraphs(
                        "The oldest ski resort in Bulgaria, Borovets lies among tall pine forests on the northern slopes of the Rila Mountains.",
                        "It began in 1896 as Chamkoria, a hunting retreat of the Bulgarian royal family. Today a gondola climbs toward Yastrebets, and the slopes suit both beginners and experienced skiers.",
                        "In summer Borovets is the starting point for the hike to Musala, the highest peak in Bulgaria and the Balkans at 2,925 metres. The resort is only about an hour's drive from Sofia, which makes it a great weekend escape.")
                },
                new Destination
                {
                    Id = 21,
                    Name = "Lisbon",
                    Country = "Portugal",
                    CategoryId = 3,
                    Latitude = 38.7223,
                    Longitude = -9.1393,
                    ImageUrl = "/images/destinations/lisbon.jpg",
                    Description = Paragraphs(
                        "Built on seven hills above the Tagus River, Lisbon is a sunny capital of colourful tiles, historic trams and ocean views.",
                        "Ride the yellow tram 28 through Alfama, the oldest district, and climb to São Jorge Castle for the view over the red rooftops. In Belém, visit the Jerónimos Monastery and the Belém Tower, two landmarks from the Age of Discovery, and try the original pastéis de Belém nearby.",
                        "In the evening, listen to fado in a small Alfama restaurant or join the crowds in the bars of Bairro Alto. Sintra, with its fairytale palaces, and the beaches of Cascais are both an easy day trip by train.")
                }
            );
        }

        private static string Paragraphs(params string[] paragraphs)
            => string.Join("\n\n", paragraphs);
    }
}
