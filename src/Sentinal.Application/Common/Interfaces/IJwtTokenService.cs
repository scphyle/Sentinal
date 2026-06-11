using System.Security.Claims;
using Sentinal.Domain.Users;

namespace Sentinal.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(UserEntity user);
    ClaimsPrincipal? ValidateToken(string token);
}