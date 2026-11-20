using Testcontainers.PostgreSql;

namespace Rabbithole.Tests.Infrastructure
{
    /// <summary>
    /// Starts one PostgreSQL container for the whole test run
    /// </summary>
    [SetUpFixture]
    public class TestContainerSetup
    {
        private static PostgreSqlContainer? _postgresContainer;
        private static string? _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new InvalidOperationException("Container has not been started. Call OneTimeSetUp first.");
                }

                return _connectionString;
            }
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _postgresContainer = new PostgreSqlBuilder("postgres:17")
                .WithDatabase("rabbithole_test_db")
                .WithUsername("test_user")
                .WithPassword("test_password")
                .WithCleanUp(true)
                .Build();

            await _postgresContainer.StartAsync();
            _connectionString = _postgresContainer.GetConnectionString();

            Console.WriteLine("Test container started");
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_postgresContainer != null)
            {
                await _postgresContainer.StopAsync();
                await _postgresContainer.DisposeAsync();
                Console.WriteLine("Test container stopped and disposed");
            }
        }
    }
}
