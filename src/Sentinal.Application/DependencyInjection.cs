using Microsoft.Extensions.DependencyInjection;

namespace Sentinal.Application;


public static class DependencyInjection
{

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection));
        });


        return services;
    }

}