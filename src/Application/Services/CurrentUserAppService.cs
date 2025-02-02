using System.Security.Claims;
using MoodTracker_back.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MoodTracker_back.Infrastructure.Adapters;

public class CurrentUserAppService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAppService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public int UserId
    {
        get
        {
            var userIdClaim = GetClaim(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new InvalidOperationException("User is not authenticated or UserId claim is invalid");
            }
            return userId;
        }
    }

    public string? UserEmail => GetClaim(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    private Claim? GetClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst(claimType);
    }
}