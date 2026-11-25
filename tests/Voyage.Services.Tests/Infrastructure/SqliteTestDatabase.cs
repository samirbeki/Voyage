using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Voyage.Data;

namespace Voyage.Services.Tests.Infrastructure
{
    public sealed class SqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection connection;

        public SqliteTestDatabase()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            connection.CreateFunction("GETUTCDATE", () => DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            DbContext = new ApplicationDbContext(options);
            DbContext.Database.EnsureCreated();
        }

        public ApplicationDbContext DbContext { get; }

        public void Dispose()
        {
            DbContext.Dispose();
            connection.Dispose();
        }
    }
}
