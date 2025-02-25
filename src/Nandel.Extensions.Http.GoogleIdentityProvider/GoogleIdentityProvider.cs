using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nandel.Extensions.Http.GoogleIdentityProvider.Managers;

namespace Nandel.Extensions.Http.GoogleIdentityProvider;

public class GoogleIdentityProvider : DelegatingHandler
{
    private readonly IOptions<GoogleIdentityProviderOptions> _options;
    private readonly IGoogleIdentityManager _identityManager;
    private readonly ILogger<GoogleIdentityProvider> _logger;

    public GoogleIdentityProvider(IOptions<GoogleIdentityProviderOptions> options, IGoogleIdentityManager identityManager, ILogger<GoogleIdentityProvider> logger)
    {
        _options = options;
        _identityManager = identityManager;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _identityManager.GetIdentityAsync(cancellationToken);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Google Identity Provider Token: {Token}", token);
        }
        
        request.Headers.Add(_options.Value.AuthorizationHeaderName, $"Bearer {token}");

        return await base.SendAsync(request, cancellationToken);
    }

    public static Func<IServiceProvider, GoogleIdentityProvider> FactoryFor(string audience)
    {
        return (services) =>
        {
            var options = services.GetRequiredService<IOptions<GoogleIdentityProviderOptions>>();
            var logger = services.GetRequiredService<ILogger<GoogleIdentityProvider>>();
            IGoogleIdentityManager identityManager = string.IsNullOrEmpty(options.Value.ServiceAccountKeyPath)
                ? GoogleIdentityManagerFromCloudServices.FactoryFor(audience).Invoke(services)
                : GoogleIdentityManagerFromCredentialsKey.FactoryFor(audience).Invoke(services);
            
            return new GoogleIdentityProvider(options, identityManager, logger);
        };
    }
}