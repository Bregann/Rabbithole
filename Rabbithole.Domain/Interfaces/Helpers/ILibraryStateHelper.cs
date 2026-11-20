using Rabbithole.Domain.DTOs.Library;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;

namespace Rabbithole.Domain.Interfaces.Helpers
{
    public interface ILibraryStateHelper
    {
        Task<LibrarySnapshot> GetSnapshot(int userId, NavidromeCredentials credentials);
        void ClearLidarrCache();
    }
}
