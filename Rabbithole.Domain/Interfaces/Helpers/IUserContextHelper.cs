namespace Rabbithole.Domain.Interfaces.Helpers
{
    public interface IUserContextHelper
    {
        int GetUserId();
        string GetUsername();
        bool IsAdmin();
    }
}
