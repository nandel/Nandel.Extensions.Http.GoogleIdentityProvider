using Microsoft.Extensions.DependencyInjection;

namespace Nandel.Extensions.Http.GoogleIdentityProvider.Configuration;

public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Add the DelegateHandler to fill the Authorization Header using a plain audience name.
    /// </summary>
    /// <param name="builder">Instance of the builder we want to add.</param>
    /// <param name="audience">The Google Cloud Service that you want to consume.</param>
    /// <returns>Builder instance with the DelegateHandler included.</returns>
    public static IHttpClientBuilder AddGoogleIdentityProvider(this IHttpClientBuilder builder, string audience)
    {
        return builder.AddHttpMessageHandler(GoogleIdentityProvider.FactoryFor(audience));
    }

    /// <summary>
    /// Add the DelegateHandler to fill the Authorization Header with a pragmatic audience name.
    /// </summary>
    /// <param name="builder">Instance of the builder we want to add.</param>
    /// <param name="audienceSelector">A Func that using the IServiceProvider you will be able to return the audience name.</param>
    /// <returns>Builder instance with the DelegateHandler included.</returns>
    public static IHttpClientBuilder AddGoogleIdentityProvider(this IHttpClientBuilder builder, Func<IServiceProvider, string> audienceSelector)
    {
        return builder.AddHttpMessageHandler(services =>
        {
            var audience = audienceSelector.Invoke(services);
            var factory = GoogleIdentityProvider.FactoryFor(audience);

            return factory.Invoke(services);
        });
    }
}