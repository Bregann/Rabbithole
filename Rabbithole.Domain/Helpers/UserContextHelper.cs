using Microsoft.AspNetCore.Http;
using Rabbithole.Domain.Interfaces.Helpers;
using System.Security.Claims;

namespace Rabbithole.Domain.Helpers
{
    public class UserContextHelper(IHttpContextAccessor httpContextAccessor) : IUserContextHelper
    {
        public int GetUserId()
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out var parsedUserId))
            {
                // Only reachable from an endpoint missing [Authorize], which is a bug
                throw new InvalidOperationException("No signed-in user on this request.");
            }

            return parsedUserId;
        }

        public string GetUsername()
        {
            return httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value
                ?? throw new InvalidOperationException("No signed-in user on this request.");
        }

        public bool IsAdmin()
        {
            return httpContextAccessor.HttpContext?.User.IsInRole(UserRoles.Admin) ?? false;
        }
    }
}
