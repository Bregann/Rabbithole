using Rabbithole.Domain.DTOs.Settings.Requests;
using Rabbithole.Domain.DTOs.Settings.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Settings
{
    public interface ISettingsService
    {
        Task<Result<GetMySettingsDto>> GetMySettings();
        Task<Result> UpdateMySettings(UpdateMySettingsRequest request);
        GetIntegrationSettingsDto GetIntegrationSettings();
        Task<Result> UpdateIntegrationSettings(UpdateIntegrationSettingsRequest request);
    }
}
