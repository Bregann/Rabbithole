using Rabbithole.Domain.DTOs.Auth;
using Rabbithole.Domain.DTOs.Auth.Requests;
using Rabbithole.Domain.DTOs.Auth.Responses;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Interfaces.Api.Auth
{
    public interface IAuthService
    {
        Task<Result<LoginResult>> Login(LoginRequest request);
        Task<Result<LoginResult>> RefreshToken(string? refreshToken);
        Task Logout(string? refreshToken);
        Task<Result<GetCurrentUserDto>> GetCurrentUser();
    }
}
