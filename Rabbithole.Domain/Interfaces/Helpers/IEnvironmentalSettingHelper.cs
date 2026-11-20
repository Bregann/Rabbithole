using Rabbithole.Domain.Enums;

namespace Rabbithole.Domain.Interfaces.Helpers
{
    public interface IEnvironmentalSettingHelper
    {
        Task LoadEnvironmentalSettings();
        string? TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum key);
        Task<bool> UpdateEnvironmentalSettingValue(EnvironmentalSettingEnum key, string newValue);
    }
}
