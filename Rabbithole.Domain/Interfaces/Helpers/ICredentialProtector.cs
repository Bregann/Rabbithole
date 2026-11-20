namespace Rabbithole.Domain.Interfaces.Helpers
{
    public interface ICredentialProtector
    {
        string Protect(string plainText);
        string Unprotect(string protectedText);
    }
}
