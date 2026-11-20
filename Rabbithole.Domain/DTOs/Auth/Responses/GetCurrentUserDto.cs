namespace Rabbithole.Domain.DTOs.Auth.Responses
{
    public class GetCurrentUserDto
    {
        public required int Id { get; set; }
        public required string Username { get; set; }
        public required bool IsAdmin { get; set; }
        public string? ListenBrainzUsername { get; set; }
    }
}
