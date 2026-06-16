using System.Security.Claims;

namespace Sentinal.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirstValue("userId");
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        throw new InvalidOperationException("User ID claim is missing or invalid");
    }
}
