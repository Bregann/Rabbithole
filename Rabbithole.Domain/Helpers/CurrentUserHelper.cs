using Rabbithole.Domain.DTOs.Library;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Interfaces.Helpers;

namespace Rabbithole.Domain.Helpers
{
    // Scoped: credentials are decrypted at most once per request
    public class CurrentUserHelper(
        IUserContextHelper userContextHelper,
        INavidromeCredentialHelper navidromeCredentialHelper,
        ILibraryStateHelper libraryStateHelper) : ICurrentUserHelper
    {
        private NavidromeCredentials? _credentials;

        public int GetUserId()
        {
            return userContextHelper.GetUserId();
        }

        public string GetUsername()
        {
            return userContextHelper.GetUsername();
        }

        public bool IsAdmin()
        {
            return userContextHelper.IsAdmin();
        }

        public async Task<NavidromeCredentials> GetCredentials()
        {
            _credentials ??= await navidromeCredentialHelper.GetCredentials(GetUserId());
            return _credentials;
        }

        public async Task<LibrarySnapshot> GetLibrarySnapshot()
        {
            return await libraryStateHelper.GetSnapshot(GetUserId(), await GetCredentials());
        }
    }
}
