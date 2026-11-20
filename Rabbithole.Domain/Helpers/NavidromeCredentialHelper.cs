using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Interfaces.Helpers;

namespace Rabbithole.Domain.Helpers
{
    public class NavidromeCredentialHelper(AppDbContext context, ICredentialProtector credentialProtector) : INavidromeCredentialHelper
    {
        public async Task<NavidromeCredentials> GetCredentials(int userId)
        {
            var user = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.NavidromeUsername, u.EncryptedNavidromePassword })
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException($"User {userId} doesn't exist.");

            return new NavidromeCredentials
            {
                Username = user.NavidromeUsername,
                Password = credentialProtector.Unprotect(user.EncryptedNavidromePassword)
            };
        }
    }
}
