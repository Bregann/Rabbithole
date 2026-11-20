using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Helpers;
using Rabbithole.Tests.Infrastructure;

namespace Rabbithole.Tests.Helpers
{
    [TestFixture]
    public class EnvironmentalSettingHelperIntegrationTests : DatabaseIntegrationTestBase
    {
        private EnvironmentalSettingHelper _environmentalSettingHelper = null!;
        private ServiceProvider _serviceProvider = null!;

        protected override async Task CustomSetUp()
        {
            await TestDatabaseSeedHelper.SeedEnvironmentalSetting(DbContext, EnvironmentalSettingEnum.NavidromeUrl, "http://navidrome:4533");
            await TestDatabaseSeedHelper.SeedEnvironmentalSetting(DbContext, EnvironmentalSettingEnum.LidarrUrl, string.Empty);

            // The helper is a singleton that creates its own scopes, so give it a real provider
            _serviceProvider = new ServiceCollection()
                .AddDbContext<AppDbContext>(options => options
                    .UseNpgsql(TestContainerSetup.ConnectionString)
                    .UseLazyLoadingProxies())
                .BuildServiceProvider();

            _environmentalSettingHelper = new EnvironmentalSettingHelper(_serviceProvider);
            await _environmentalSettingHelper.LoadEnvironmentalSettings();
        }

        // A [TearDown] method (not the CustomTearDown override) so NUnit's analyzer can see the provider is disposed
        [TearDown]
        public async Task DisposeServiceProvider()
        {
            await _serviceProvider.DisposeAsync();
        }

        [Test]
        public void TryGetEnvironmentalSettingValue_WhenSettingHasValue_ShouldReturnValue()
        {
            // Act
            var result = _environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.NavidromeUrl);

            // Assert
            Assert.That(result, Is.EqualTo("http://navidrome:4533"));
        }

        [Test]
        public void TryGetEnvironmentalSettingValue_WhenSettingIsBlank_ShouldReturnNull()
        {
            // Act
            var result = _environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.LidarrUrl);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TryGetEnvironmentalSettingValue_WhenSettingDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = _environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.AudioMuseUrl);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task UpdateEnvironmentalSettingValue_WhenSettingExists_ShouldPersistAndUpdateCache()
        {
            // Act
            var result = await _environmentalSettingHelper.UpdateEnvironmentalSettingValue(EnvironmentalSettingEnum.NavidromeUrl, "http://192.168.1.100:4533");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.NavidromeUrl), Is.EqualTo("http://192.168.1.100:4533"));

            var saved = await DbContext.EnvironmentalSettings.AsNoTracking().SingleAsync(s => s.Key == nameof(EnvironmentalSettingEnum.NavidromeUrl));
            Assert.That(saved.Value, Is.EqualTo("http://192.168.1.100:4533"));
        }

        [Test]
        public async Task UpdateEnvironmentalSettingValue_WhenSettingDoesNotExist_ShouldReturnFalse()
        {
            // Act
            var result = await _environmentalSettingHelper.UpdateEnvironmentalSettingValue(EnvironmentalSettingEnum.AudioMuseUrl, "http://audiomuse:8000");

            // Assert
            Assert.That(result, Is.False);
            Assert.That(await DbContext.EnvironmentalSettings.AnyAsync(s => s.Key == nameof(EnvironmentalSettingEnum.AudioMuseUrl)), Is.False);
        }
    }
}
