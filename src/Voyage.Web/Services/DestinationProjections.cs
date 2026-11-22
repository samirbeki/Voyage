using System.Linq.Expressions;
using Voyage.Data.Models;
using Voyage.Web.ViewModels.Destinations;

namespace Voyage.Web.Services
{
    public static class DestinationProjections
    {
        public static readonly Expression<Func<Destination, DestinationAllViewModel>> ToCard =
            d => new DestinationAllViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Country = d.Country,
                ImageUrl = d.ImageUrl,
                CategoryName = d.Category.Name,
                Description = d.Description,
                ReviewsCount = d.Reviews.Count(r => !r.IsHidden),
                AverageRating = d.Reviews
                    .Where(r => !r.IsHidden)
                    .Average(r => (double?)r.Rating)
            };
    }
}