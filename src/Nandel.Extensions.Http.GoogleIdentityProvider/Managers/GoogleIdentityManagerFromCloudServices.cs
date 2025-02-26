using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nandel.Extensions.Http.GoogleIdentityProvider.Managers;

/// <summary>
/// Strategy to recover the identity from inside Google Cloud Services, like VMs or Cloud Run instances
///
/// OBS 1: This is the default strategy if CredentialsKey is not informed in the options
/// OBS 2: This strategy does not require a CredentialsKey, but, it MUST be executed from inside GCP
/// </summary>
public class GoogleIdentityManagerFromCloudServices : IGoogleIdentityManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _audience;
    private readonly ILogger<GoogleIdentityManagerFromCloudServices> _logger;
    private readonly SemaphoreSlim _semaphore = new(initialCount: 0, maxCount:1);

    public GoogleIdentityManagerFromCloudServices(IHttpClientFactory httpClientFactory, string audience, ILogger<GoogleIdentityManagerFromCloudServices> logger)
    {
        _httpClientFactory = httpClientFactory;
        _audience = audience;
        _logger = logger;
    }

    private JwtSecurityToken? _token;
    
    public async Task<string> GetIdentityAsync(CancellationToken cancel = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancel);
            if (_token != null && _token.ValidTo > DateTime.UtcNow)
            {
                return _token.RawData;
            }
            
            _token = await FetchIdentityTokenAsync(cancel);
            return _token.RawData;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<JwtSecurityToken> FetchIdentityTokenAsync(CancellationToken cancel = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Fetching identity token");
        }
        
        var req = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/identity?audience={_audience}"),
            Headers = {{ "Metadata-Flavor", "Google" }},
        };

        using var http = _httpClientFactory.CreateClient("google-identity-provider");
        var res = await http.SendAsync(req, cancel);
        if (!res.IsSuccessStatusCode)
        {
            throw new Exception("Unable to get identity token from Google");
        }

        var rawIdentityToken = await res.Content.ReadAsStringAsync(cancel);
        return new JwtSecurityToken(rawIdentityToken);
    }
    
    public static Func<IServiceProvider, GoogleIdentityManagerFromCloudServices> FactoryFor(string audience)
    {
        return (services) =>
        {
            var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            var logger = services.GetRequiredService<ILogger<GoogleIdentityManagerFromCloudServices>>();

            return new GoogleIdentityManagerFromCloudServices(httpClientFactory, audience, logger);
        };
    }
}