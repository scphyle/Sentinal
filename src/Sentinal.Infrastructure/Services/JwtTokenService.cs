using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Domain.Users;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace Sentinal.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenService> _logger;
    public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger)
    {
        _config = config;
        _logger = logger;
    }
    public string GenerateToken(UserEntity user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)

        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                token,
                new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true
                }, out _);
            return principal;
        }catch(Exception e)
        {
            _logger.LogError(e, "Error validating token");
            return null;
        }
    }
}