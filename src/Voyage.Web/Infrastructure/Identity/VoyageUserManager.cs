using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Voyage.Web.Data;

namespace Voyage.Web.Infrastructure.Identity
{
    public class VoyageUserManager : UserManager<IdentityUser>
    {
        private readonly ApplicationDbContext dbContext;

        public VoyageUserManager(
            IUserStore<IdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<IdentityUser> passwordHasher,
            IEnumerable<IUserValidator<IdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<IdentityUser>> logger,
            ApplicationDbContext dbContext)
            : base(
                store,
                optionsAccessor,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                services,
                logger)
        {
            this.dbContext = dbContext;
        }

        public override async Task<IdentityResult> DeleteAsync(IdentityUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            await using var transaction = dbContext.Database.IsRelational()
                ? await dbContext.Database.BeginTransactionAsync()
                : null;

            var reviews = await dbContext.Reviews
                .IgnoreQueryFilters()
                .Where(r => r.AuthorId == user.Id)
                .ToListAsync();

            var favorites = await dbContext.Favorites
                .IgnoreQueryFilters()
                .Where(f => f.UserId == user.Id)
                .ToListAsync();

            var packingItems = await dbContext.PackingItems
                .Where(p => p.Trip.OwnerId == user.Id)
                .ToListAsync();

            var trips = await dbContext.Trips
                .Where(t => t.OwnerId == user.Id)
                .ToListAsync();

            dbContext.Reviews.RemoveRange(reviews);
            dbContext.Favorites.RemoveRange(favorites);
            dbContext.PackingItems.RemoveRange(packingItems);
            dbContext.Trips.RemoveRange(trips);

            await dbContext.SaveChangesAsync();

            var result = await base.DeleteAsync(user);

            if (result.Succeeded && transaction != null)
            {
                await transaction.CommitAsync();
            }

            return result;
        }
    }
}
