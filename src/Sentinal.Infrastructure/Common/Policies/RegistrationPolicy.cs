using Microsoft.Extensions.Configuration;
using Sentinal.Application.Common.Interfaces;

namespace Sentinal.Infrastructure.Common.Policies;

public class RegistrationPolicy : IRegistrationPolicy
{
    private readonly IConfiguration _configuration;

    public RegistrationPolicy(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsRegistrationEnabled()
    {
        var enabledValue = _configuration["Authentication:EnableUserRegistration"];
        return string.IsNullOrEmpty(enabledValue) || bool.Parse(enabledValue);
    }
}
