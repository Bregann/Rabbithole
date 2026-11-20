using Rabbithole.Domain.Database.Models;

namespace Rabbithole.Domain.Interfaces.Helpers
{
    public interface IJwtTokenHelper
    {
        string CreateAccessToken(User user);
        string CreateRefreshToken();
        string HashToken(string token);
    }
}
