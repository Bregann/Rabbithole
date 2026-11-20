using Rabbithole.Domain.DTOs.Status.Responses;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Interfaces.Api.Status;
using Rabbithole.Domain.Interfaces.Helpers;
using System.Reflection;

namespace Rabbithole.Domain.Services.Status
{
    public class StatusService(IEnvironmentalSettingHelper environmentalSettingHelper) : IStatusService
    {
        // The settings each integration needs before Rabbithole can talk to it
        private static readonly Dictionary<IntegrationType, EnvironmentalSettingEnum[]> _requiredSettings = new()
        {
            { IntegrationType.Navidrome, [EnvironmentalSettingEnum.NavidromeUrl] },
            { IntegrationType.ListenBrainz, [EnvironmentalSettingEnum.ListenBrainzApiUrl] },
            { IntegrationType.Lidarr, [EnvironmentalSettingEnum.LidarrUrl, EnvironmentalSettingEnum.LidarrApiKey] },
            { IntegrationType.AudioMuse, [EnvironmentalSettingEnum.AudioMuseUrl] },
        };

        public GetStatusDto GetStatus()
        {
            var integrations = _requiredSettings
                .Select(i => new IntegrationStatusItem
                {
                    Integration = i.Key,
                    IsConfigured = i.Value.All(setting => environmentalSettingHelper.TryGetEnvironmentalSettingValue(setting) != null)
                })
                .ToArray();

            return new GetStatusDto
            {
                Version = GetVersion(),
                IsFullyConfigured = integrations.All(i => i.IsConfigured),
                Integrations = integrations
            };
        }

        private static string GetVersion()
        {
            var version = typeof(StatusService).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "0.0.0";

            // Strip the "+commitsha" suffix the SDK appends
            return version.Split('+')[0];
        }
    }
}
