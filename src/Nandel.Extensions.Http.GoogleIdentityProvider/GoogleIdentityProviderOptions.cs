namespace Nandel.Extensions.Http.GoogleIdentityProvider;

public class GoogleIdentityProviderOptions
{
    public string? ServiceAccountKeyPath { get; set; }
    public required string AuthorizationHeaderName { get; set; } = "Authorization";
}