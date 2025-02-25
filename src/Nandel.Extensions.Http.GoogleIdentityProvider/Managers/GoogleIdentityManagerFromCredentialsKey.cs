using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nandel.Extensions.Http.GoogleIdentityProvider.Managers;

/// <summary>
/// Strategy to recover the Identity from a Credential Key File
/// </summary>
public class GoogleIdentityManagerFromCredentialsKey : IGoogleIdentityManager
{
    private readonly IOptions<GoogleIdentityProviderOptions> _options;
    private readonly string _audience;
    private readonly ILogger<GoogleIdentityManagerFromCredentialsKey> _logger;

    public GoogleIdentityManagerFromCredentialsKey(IOptions<GoogleIdentityProviderOptions> options, string audience, ILogger<GoogleIdentityManagerFromCredentialsKey> logger)
    {
        _options = options;
        _audience = audience;
        _logger = logger;
    }

    private ITokenAccess? _tokenAccess;
    
    public async Task<string> GetIdentityAsync(CancellationToken cancel = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Fetching identity token");
        }
        
        var keyPath = _options.Value.ServiceAccountKeyPath;
        _tokenAccess ??= (await GoogleCredential.FromFileAsync(keyPath, cancel))
            .CreateScoped(_audience);
        
        return await _tokenAccess.GetAccessTokenForRequestAsync(cancellationToken: cancel);
    }

    public static Func<IServiceProvider, GoogleIdentityManagerFromCredentialsKey> FactoryFor(string audience)
    {
        return (services) =>
        {
            var options = services.GetRequiredService<IOptions<GoogleIdentityProviderOptions>>();
            var logger = services.GetRequiredService<ILogger<GoogleIdentityManagerFromCredentialsKey>>();

            return new GoogleIdentityManagerFromCredentialsKey(options, audience, logger);
        };
    }
}