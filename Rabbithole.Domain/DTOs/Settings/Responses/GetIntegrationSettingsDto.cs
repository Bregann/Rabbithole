namespace Rabbithole.Domain.DTOs.Settings.Responses
{
    public class GetIntegrationSettingsDto
    {
        public required IntegrationSettingItem[] Settings { get; set; } = [];
    }

    public class IntegrationSettingItem
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
        public required bool IsSecret { get; set; }
        public required bool HasValue { get; set; }
    }
}
