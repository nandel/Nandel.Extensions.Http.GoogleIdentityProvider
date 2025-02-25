namespace Nandel.Extensions.Http.GoogleIdentityProvider;

public interface IGoogleIdentityManager
{
    public Task<string> GetIdentityAsync(CancellationToken cancel = default);
}