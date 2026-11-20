namespace Rabbithole.Domain.DTOs.Settings.Responses
{
    public class GetMySettingsDto
    {
        public required string Username { get; set; }
        public required bool IsAdmin { get; set; }
        public string? ListenBrainzUsername { get; set; }
    }
}
