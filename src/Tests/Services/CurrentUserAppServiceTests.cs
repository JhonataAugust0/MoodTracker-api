using System.Security.Claims;
using Moq;
using Xunit;
using MoodTracker_back.Infrastructure.Adapters;

namespace MoodTracker_back.Tests.Services
{
    public class CurrentUserAppServiceTests
    {
        [Fact]
        public void UserId_ReturnsValidId_WhenAuthenticated()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123")
            }));
            
            var accessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
            var service = new CurrentUserAppService(accessor);

            var userId = service.UserId;

            Assert.Equal(123, userId);
        }
    }
}