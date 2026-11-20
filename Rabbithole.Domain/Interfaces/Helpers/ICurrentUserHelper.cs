using Rabbithole.Domain.DTOs.Library;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;

namespace Rabbithole.Domain.Interfaces.Helpers
{
    // Everything a request-scoped service needs about the signed-in user, in one place
    public interface ICurrentUserHelper
    {
        int GetUserId();
        string GetUsername();
        bool IsAdmin();
        Task<NavidromeCredentials> GetCredentials();
        Task<LibrarySnapshot> GetLibrarySnapshot();
    }
}
