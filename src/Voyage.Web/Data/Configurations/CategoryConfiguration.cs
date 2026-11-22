using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voyage.Web.Data.Entities;

namespace Voyage.Web.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasData(
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
        }
    }
}