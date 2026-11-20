namespace Rabbithole.Domain.DTOs.Settings.Requests
{
    public class UpdateIntegrationSettingsRequest
    {
        public required UpdateIntegrationSettingItem[] Settings { get; set; } = [];
    }

    public class UpdateIntegrationSettingItem
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
    }
}
