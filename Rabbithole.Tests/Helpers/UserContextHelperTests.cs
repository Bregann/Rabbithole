using Microsoft.AspNetCore.Http;
using Moq;
using Rabbithole.Domain.Helpers;
using MockFactory = Rabbithole.Tests.Infrastructure.MockFactory;

namespace Rabbithole.Tests.Helpers
{
    [TestFixture]
    public class UserContextHelperTests
    {
        [Test]
        public void GetUserId_WhenSignedIn_ShouldReturnIdFromClaims()
        {
            // Arrange
            var userContextHelper = new UserContextHelper(MockFactory.CreateHttpContextAccessor(userId: "42").Object);

            // Act
            var result = userContextHelper.GetUserId();

            // Assert
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void GetUsername_WhenSignedIn_ShouldReturnUsernameFromClaims()
        {
            // Arrange
            var userContextHelper = new UserContextHelper(MockFactory.CreateHttpContextAccessor(username: "dave").Object);

            // Act
            var result = userContextHelper.GetUsername();

            // Assert
            Assert.That(result, Is.EqualTo("dave"));
        }

        [Test]
        public void GetUserId_WhenNotSignedIn_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var userContextHelper = new UserContextHelper(accessor.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => userContextHelper.GetUserId());
        }
    }
}
