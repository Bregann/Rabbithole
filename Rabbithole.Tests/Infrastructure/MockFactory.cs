using Microsoft.AspNetCore.Http;
using Moq;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Interfaces.Helpers;
using System.Security.Claims;

namespace Rabbithole.Tests.Infrastructure
{
    /// <summary>
    /// Mocks for external dependencies. Never mock the database.
    /// </summary>
    public static class MockFactory
    {
        public static Mock<IHttpContextAccessor> CreateHttpContextAccessor(string userId = "1", string username = "testuser")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username)
            };

            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            };

            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(x => x.HttpContext).Returns(context);

            return mock;
        }

        public static Mock<IUserContextHelper> CreateUserContextHelper(int userId = 1, string username = "testuser")
        {
            var mock = new Mock<IUserContextHelper>();
            mock.Setup(x => x.GetUserId()).Returns(userId);
            mock.Setup(x => x.GetUsername()).Returns(username);

            return mock;
        }

        /// <summary>
        /// Every setting returns a value unless it's listed in <paramref name="missingSettings"/>
        /// </summary>
        public static Mock<IEnvironmentalSettingHelper> CreateEnvironmentalSettingHelper(params EnvironmentalSettingEnum[] missingSettings)
        {
            var mock = new Mock<IEnvironmentalSettingHelper>();

            foreach (var key in Enum.GetValues<EnvironmentalSettingEnum>())
            {
                var value = missingSettings.Contains(key) ? null : $"test-{key}";
                mock.Setup(x => x.TryGetEnvironmentalSettingValue(key)).Returns(value);
            }

            return mock;
        }
    }
}
