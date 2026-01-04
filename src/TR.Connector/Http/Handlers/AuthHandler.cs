using System.Net.Http.Headers;
using TR.Connector.Http.JwtToken;

namespace TR.Connector.Http.Handlers;

public class AuthHandler : DelegatingHandler
{
    private readonly ITokenStore tokenStore;

    public AuthHandler(ITokenStore tokenStore)
    {
        this.tokenStore = tokenStore;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(tokenStore.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", tokenStore.AccessToken);
        }
        return base.SendAsync(request, cancellationToken);
    }
}