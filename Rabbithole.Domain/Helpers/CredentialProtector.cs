using Microsoft.AspNetCore.DataProtection;
using Rabbithole.Domain.Interfaces.Helpers;
using System.Security.Cryptography;

namespace Rabbithole.Domain.Helpers
{
    // Navidrome passwords must be reversible (Subsonic token auth needs the plain password),
    // so they're encrypted with ASP.NET Data Protection. Keys live in the database (see AppDbContext).
    public class CredentialProtector(IDataProtectionProvider dataProtectionProvider) : ICredentialProtector
    {
        private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("Rabbithole.NavidromePassword.v1");

        public string Protect(string plainText)
        {
            return _protector.Protect(plainText);
        }

        public string Unprotect(string protectedText)
        {
            try
            {
                return _protector.Unprotect(protectedText);
            }
            catch (CryptographicException ex)
            {
                // Only happens if the Data Protection keys were lost. Signing in again re-encrypts the password.
                throw new InvalidOperationException("The saved Navidrome password can't be decrypted. Sign out and in again.", ex);
            }
        }
    }
}
