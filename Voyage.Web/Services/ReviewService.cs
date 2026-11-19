using Microsoft.EntityFrameworkCore;
using Voyage.Web.Common;
using Voyage.Web.Data;
using Voyage.Web.Data.Entities;
using Voyage.Web.Services.Contracts;
using Voyage.Web.ViewModels.Reviews;

namespace Voyage.Web.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext dbContext;

        public ReviewService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<ReviewViewModel>> GetForDestinationAsync(
            int destinationId, string? userId, bool includeHidden)
        {
            return await dbContext.Reviews
                .AsNoTracking()
                .Where(r =>
                    r.DestinationId == destinationId &&
                    (includeHidden || !r.IsHidden))
                .OrderByDescending(r => r.CreatedOn)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    AuthorName = r.Author.UserName!,
                    CreatedOn = r.CreatedOn,
                    IsEdited = r.ModifiedOn != null,
                    IsMine = userId != null && r.AuthorId == userId,
                    IsHidden = r.IsHidden
                })
                .ToListAsync();
        }

        public async Task<RatingSummaryViewModel> GetRatingSummaryAsync(int destinationId)
        {
            var ratings = dbContext.Reviews
                .Where(r => r.DestinationId == destinationId && !r.IsHidden);

            int count = await ratings.CountAsync();

            return new RatingSummaryViewModel
            {
                Count = count,
                Average = count == 0
                    ? 0
                    : Math.Round(await ratings.AverageAsync(r => r.Rating), 1)
            };
        }

        public async Task<bool> HasUserReviewedAsync(int destinationId, string userId)
        {
            return await dbContext.Reviews
                .AnyAsync(r =>
                    r.DestinationId == destinationId &&
                    r.AuthorId == userId);
        }

        public async Task<bool> CreateAsync(
            int destinationId, ReviewFormModel model, string userId)
        {
            bool destinationExists = await dbContext.Destinations
                .AnyAsync(d => d.Id == destinationId);

            if (!destinationExists ||
                await HasUserReviewedAsync(destinationId, userId))
            {
                return false;
            }

            var review = new Review
            {
                DestinationId = destinationId,
                AuthorId = userId,
                Rating = model.Rating,
                Comment = model.Comment.Trim(),
                CreatedOn = DateTime.UtcNow
            };

            await dbContext.Reviews.AddAsync(review);
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ReviewEditViewModel?> GetForEditAsync(int id, string userId)
        {
            return await dbContext.Reviews
                .AsNoTracking()
                .Where(r => r.Id == id && r.AuthorId == userId)
                .Select(r => new ReviewEditViewModel
                {
                    Id = r.Id,
                    DestinationId = r.DestinationId,
                    DestinationName = r.Destination.Name,
                    Rating = r.Rating,
                    Comment = r.Comment
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int?> EditAsync(
            int id, ReviewFormModel model, string userId)
        {
            var review = await dbContext.Reviews
                .FirstOrDefaultAsync(r =>
                    r.Id == id && r.AuthorId == userId);

            if (review == null)
            {
                return null;
            }

            review.Rating = model.Rating;
            review.Comment = model.Comment.Trim();
            review.ModifiedOn = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return review.DestinationId;
        }

        public async Task<int?> DeleteAsync(int id, string userId)
        {
            var review = await dbContext.Reviews
                .FirstOrDefaultAsync(r =>
                    r.Id == id && r.AuthorId == userId);

            if (review == null)
            {
                return null;
            }

            review.IsDeleted = true;

            await dbContext.SaveChangesAsync();

            return review.DestinationId;
        }

        public async Task<ModerationReviewsPageViewModel> GetForModerationAsync(
            string status, int page)
        {
            status = status?.ToLowerInvariant() switch
            {
                "visible" => "visible",
                "hidden" => "hidden",
                _ => "all"
            };

            var reviewsQuery = dbContext.Reviews
                .AsNoTracking();

            if (status == "visible")
            {
                reviewsQuery = reviewsQuery.Where(r => !r.IsHidden);
            }
            else if (status == "hidden")
            {
                reviewsQuery = reviewsQuery.Where(r => r.IsHidden);
            }

            int totalCount = await reviewsQuery.CountAsync();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    totalCount / (double)ValidationConstants.ModerationReviewsPageSize));

            page = Math.Clamp(page, 1, totalPages);

            var reviews = await reviewsQuery
                .OrderByDescending(r => r.CreatedOn)
                .Skip((page - 1) * ValidationConstants.ModerationReviewsPageSize)
                .Take(ValidationConstants.ModerationReviewsPageSize)
                .Select(r => new ModerationReviewViewModel
                {
                    Id = r.Id,
                    DestinationId = r.DestinationId,
                    DestinationName = r.Destination.Name,
                    AuthorName = r.Author.UserName!,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn,
                    IsHidden = r.IsHidden
                })
                .ToListAsync();

            return new ModerationReviewsPageViewModel
            {
                Reviews = reviews,
                Status = status,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<bool> SetHiddenAsync(int id, bool isHidden)
        {
            var review = await dbContext.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return false;
            }

            review.IsHidden = isHidden;

            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteByModeratorAsync(int id)
        {
            var review = await dbContext.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return false;
            }

            review.IsDeleted = true;

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}