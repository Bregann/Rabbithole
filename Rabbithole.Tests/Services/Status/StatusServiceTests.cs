using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Services.Status;
using Rabbithole.Tests.Infrastructure;

namespace Rabbithole.Tests.Services.Status
{
    [TestFixture]
    public class StatusServiceTests
    {
        [Test]
        public void GetStatus_WhenEverythingIsConfigured_ShouldBeFullyConfigured()
        {
            // Arrange
            var statusService = new StatusService(MockFactory.CreateEnvironmentalSettingHelper().Object);

            // Act
            var result = statusService.GetStatus();

            // Assert
            Assert.That(result.IsFullyConfigured, Is.True);
            Assert.That(result.Integrations, Has.Length.EqualTo(Enum.GetValues<IntegrationType>().Length));
            Assert.That(result.Integrations.All(i => i.IsConfigured), Is.True);
        }

        [Test]
        public void GetStatus_WhenLidarrApiKeyIsMissing_ShouldMarkOnlyLidarrAsNotConfigured()
        {
            // Arrange
            var statusService = new StatusService(MockFactory.CreateEnvironmentalSettingHelper(EnvironmentalSettingEnum.LidarrApiKey).Object);

            // Act
            var result = statusService.GetStatus();

            // Assert
            Assert.That(result.IsFullyConfigured, Is.False);
            Assert.That(result.Integrations.Single(i => i.Integration == IntegrationType.Lidarr).IsConfigured, Is.False);
            Assert.That(result.Integrations.Where(i => i.Integration != IntegrationType.Lidarr).All(i => i.IsConfigured), Is.True);
        }

        [Test]
        public void GetStatus_ShouldReturnVersionWithoutCommitSuffix()
        {
            // Arrange
            var statusService = new StatusService(MockFactory.CreateEnvironmentalSettingHelper().Object);

            // Act
            var result = statusService.GetStatus();

            // Assert
            Assert.That(result.Version, Is.Not.Empty);
            Assert.That(result.Version, Does.Not.Contain("+"));
        }
    }
}
