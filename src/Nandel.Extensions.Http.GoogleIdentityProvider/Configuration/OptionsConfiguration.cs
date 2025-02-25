using Microsoft.Extensions.DependencyInjection;

namespace Nandel.Extensions.Http.GoogleIdentityProvider.Configuration;

public static class OptionsConfiguration
{
    /// <summary>
    /// Configure the Google Identity Provider
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationSection"></param>
    /// <returns></returns>
    public static IServiceCollection AddGoogleIdentityProviderOptions(this IServiceCollection services, string configurationSection)
    {
        services
            .AddOptions<GoogleIdentityProviderOptions>()
            .BindConfiguration(configurationSection);

        return services;
    }
}