using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.DTOs.Settings.Requests;
using Rabbithole.Domain.DTOs.Settings.Responses;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Interfaces.Api.Settings;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Services.Settings
{
    public class SettingsService(
        AppDbContext context,
        IUserContextHelper userContextHelper,
        IListenBrainzApiClient listenBrainzApiClient,
        IEnvironmentalSettingHelper environmentalSettingHelper,
        ILibraryStateHelper libraryStateHelper) : ISettingsService
    {
        // Admins can edit these from the UI. Hangfire credentials stay env-only.
        private static readonly EnvironmentalSettingEnum[] _editableSettings =
        [
            EnvironmentalSettingEnum.NavidromeUrl,
            EnvironmentalSettingEnum.ListenBrainzApiUrl,
            EnvironmentalSettingEnum.LidarrUrl,
            EnvironmentalSettingEnum.LidarrApiKey,
            EnvironmentalSettingEnum.AudioMuseUrl,
            EnvironmentalSettingEnum.AudioMuseApiToken,
            EnvironmentalSettingEnum.WeeklyRequestCap
        ];

        private static readonly EnvironmentalSettingEnum[] _secretSettings =
        [
            EnvironmentalSettingEnum.LidarrApiKey,
            EnvironmentalSettingEnum.AudioMuseApiToken
        ];

        public async Task<Result<GetMySettingsDto>> GetMySettings()
        {
            var userId = userContextHelper.GetUserId();

            var settings = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new GetMySettingsDto
                {
                    Username = u.NavidromeUsername,
                    IsAdmin = u.IsAdmin,
                    ListenBrainzUsername = u.ListenBrainzUsername
                })
                .FirstOrDefaultAsync();

            return settings == null
                ? Result.NotFoundFail<GetMySettingsDto>("Couldn't find your account.")
                : Result.Ok(settings);
        }

        public async Task<Result> UpdateMySettings(UpdateMySettingsRequest request)
        {
            var userId = userContextHelper.GetUserId();
            var listenBrainzUsername = string.IsNullOrWhiteSpace(request.ListenBrainzUsername) ? null : request.ListenBrainzUsername.Trim();

            if (listenBrainzUsername != null && !await listenBrainzApiClient.UserExists(listenBrainzUsername))
            {
                return Result.Fail($"ListenBrainz doesn't have a user called \"{listenBrainzUsername}\".");
            }

            var changes = await context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.ListenBrainzUsername, listenBrainzUsername));

            return changes == 0
                ? Result.NotFoundFail("Couldn't find your account.")
                : Result.Ok();
        }

        public GetIntegrationSettingsDto GetIntegrationSettings()
        {
            return new GetIntegrationSettingsDto
            {
                Settings = _editableSettings
                    .Select(key =>
                    {
                        var value = environmentalSettingHelper.TryGetEnvironmentalSettingValue(key);
                        var isSecret = _secretSettings.Contains(key);

                        return new IntegrationSettingItem
                        {
                            Key = key.ToString(),
                            Value = isSecret ? string.Empty : value ?? string.Empty,
                            IsSecret = isSecret,
                            HasValue = value != null
                        };
                    })
                    .ToArray()
            };
        }

        public async Task<Result> UpdateIntegrationSettings(UpdateIntegrationSettingsRequest request)
        {
            foreach (var setting in request.Settings)
            {
                if (!Enum.TryParse<EnvironmentalSettingEnum>(setting.Key, out var key) || !_editableSettings.Contains(key))
                {
                    return Result.Fail($"\"{setting.Key}\" isn't a setting you can change here.");
                }

                var value = setting.Value.Trim();

                // Secrets are never sent to the browser, so a blank secret means "leave it alone"
                if (_secretSettings.Contains(key) && value.Length == 0)
                {
                    continue;
                }

                if (key == EnvironmentalSettingEnum.WeeklyRequestCap && (!int.TryParse(value, out var cap) || cap < 0))
                {
                    return Result.Fail("The weekly request cap must be a whole number, 0 or more.");
                }

                await environmentalSettingHelper.UpdateEnvironmentalSettingValue(key, value);
            }

            libraryStateHelper.ClearLidarrCache();

            return Result.Ok();
        }
    }
}
