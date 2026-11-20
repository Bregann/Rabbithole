using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;

namespace Rabbithole.Tests.Infrastructure
{
    /// <summary>
    /// Base class for integration tests that need a real database.
    /// Every test gets a fresh database built from the migrations.
    /// </summary>
    public abstract class DatabaseIntegrationTestBase
    {
        protected AppDbContext DbContext { get; private set; } = null!;

        protected DbContextOptions<AppDbContext> DbContextOptions { get; private set; } = null!;

        [SetUp]
        public async Task SetUp()
        {
            DbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(TestContainerSetup.ConnectionString)
                .UseLazyLoadingProxies()
                .EnableSensitiveDataLogging()
                .Options;

            DbContext = new AppDbContext(DbContextOptions);

            await DbContext.Database.EnsureDeletedAsync();

            // MigrateAsync (not EnsureCreatedAsync) so the migrations themselves are tested
            await DbContext.Database.MigrateAsync();

            await CustomSetUp();
        }

        [TearDown]
        public async Task TearDown()
        {
            await CustomTearDown();
            await DbContext.DisposeAsync();
        }

        /// <summary>
        /// Override to seed data and create the service under test
        /// </summary>
        protected virtual Task CustomSetUp()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to add custom teardown logic
        /// </summary>
        protected virtual Task CustomTearDown()
        {
            return Task.CompletedTask;
        }
    }
}
