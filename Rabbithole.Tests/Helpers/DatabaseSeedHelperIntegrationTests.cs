using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Helpers;
using Rabbithole.Tests.Infrastructure;

namespace Rabbithole.Tests.Helpers
{
    [TestFixture]
    public class DatabaseSeedHelperIntegrationTests : DatabaseIntegrationTestBase
    {
        [Test]
        public async Task SeedEnvironmentalSettings_OnEmptyDatabase_ShouldCreateEverySettingWithDefaults()
        {
            // Act
            await DatabaseSeedHelper.SeedEnvironmentalSettings(DbContext, _ => null);

            // Assert
            var settings = await DbContext.EnvironmentalSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);
            Assert.That(settings, Has.Count.EqualTo(Enum.GetValues<EnvironmentalSettingEnum>().Length));
            Assert.That(settings[nameof(EnvironmentalSettingEnum.ListenBrainzApiUrl)], Is.EqualTo("https://api.listenbrainz.org"));
            Assert.That(settings[nameof(EnvironmentalSettingEnum.WeeklyRequestCap)], Is.EqualTo("5"));
            Assert.That(settings[nameof(EnvironmentalSettingEnum.NavidromeUrl)], Is.Empty);
        }

        [Test]
        public async Task SeedEnvironmentalSettings_WithEnvironmentVariable_ShouldUseItForNewSetting()
        {
            // Act
            await DatabaseSeedHelper.SeedEnvironmentalSettings(DbContext, key => key == nameof(EnvironmentalSettingEnum.NavidromeUrl) ? "http://navidrome:4533" : null);

            // Assert
            var setting = await DbContext.EnvironmentalSettings.AsNoTracking().SingleAsync(s => s.Key == nameof(EnvironmentalSettingEnum.NavidromeUrl));
            Assert.That(setting.Value, Is.EqualTo("http://navidrome:4533"));
        }

        [Test]
        public async Task SeedEnvironmentalSettings_WithEnvironmentVariable_ShouldOverrideExistingValue()
        {
            // Arrange
            await TestDatabaseSeedHelper.SeedEnvironmentalSetting(DbContext, EnvironmentalSettingEnum.LidarrUrl, "http://old-lidarr:8686");

            // Act
            await DatabaseSeedHelper.SeedEnvironmentalSettings(DbContext, key => key == nameof(EnvironmentalSettingEnum.LidarrUrl) ? "http://lidarr:8686" : null);

            // Assert
            var setting = await DbContext.EnvironmentalSettings.AsNoTracking().SingleAsync(s => s.Key == nameof(EnvironmentalSettingEnum.LidarrUrl));
            Assert.That(setting.Value, Is.EqualTo("http://lidarr:8686"));
        }

        [Test]
        public async Task SeedEnvironmentalSettings_WithoutEnvironmentVariable_ShouldKeepExistingValue()
        {
            // Arrange
            await TestDatabaseSeedHelper.SeedEnvironmentalSetting(DbContext, EnvironmentalSettingEnum.LidarrUrl, "http://lidarr:8686");

            // Act
            await DatabaseSeedHelper.SeedEnvironmentalSettings(DbContext, _ => null);

            // Assert
            var setting = await DbContext.EnvironmentalSettings.AsNoTracking().SingleAsync(s => s.Key == nameof(EnvironmentalSettingEnum.LidarrUrl));
            Assert.That(setting.Value, Is.EqualTo("http://lidarr:8686"));
        }
    }
}
