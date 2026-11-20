using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;

namespace Rabbithole.Domain.Interfaces.Helpers
{
    public interface INavidromeCredentialHelper
    {
        Task<NavidromeCredentials> GetCredentials(int userId);
    }
}
