using Rabbithole.Domain.DTOs.Auth.Responses;

namespace Rabbithole.Domain.DTOs.Auth
{
    public class LoginResult
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required GetCurrentUserDto User { get; set; }
    }
}
