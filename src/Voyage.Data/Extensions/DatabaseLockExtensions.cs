using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Voyage.Data.Extensions
{
    public static class DatabaseLockExtensions
    {
        private const int LockTimeoutMilliseconds = 10000;

        public static async Task<IDbContextTransaction?> BeginLockedTransactionAsync(
            this DatabaseFacade database,
            string resource)
        {
            if (!database.IsSqlServer())
            {
                return null;
            }

            var transaction = await database.BeginTransactionAsync();

            try
            {
                await database.ExecuteSqlInterpolatedAsync($@"
DECLARE @result int;
EXEC @result = sp_getapplock
    @Resource = {resource},
    @LockMode = 'Exclusive',
    @LockOwner = 'Transaction',
    @LockTimeout = {LockTimeoutMilliseconds};
IF @result < 0
    RAISERROR('Could not acquire the application lock.', 16, 1);");
            }
            catch
            {
                await transaction.DisposeAsync();
                throw;
            }

            return transaction;
        }
    }
}
