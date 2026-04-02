using System.Net.Http.Headers;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Onyx.Oms.Infrastructure.Identity.IdP;

public class IdPTokenHandler : DelegatingHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthenticationOptions _authOptions;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "IdP_M2M_Token";

    public IdPTokenHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthenticationOptions> authOptions,
        IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _authOptions = authOptions.Value;
        _cache = cache;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out string? cachedToken))
        {
            return cachedToken;
        }

        var client = _httpClientFactory.CreateClient();

        // 1. Get Discovery Document
        var disco = await client.GetDiscoveryDocumentAsync(_authOptions.Authority, cancellationToken);
        if (disco.IsError)
        {
            throw new Exception($"Failed to discover IdP endpoints: {disco.Error}");
        }

        // 2. Request Token
        // NOTE: We need ClientId/Secret for the BACKEND (M2M), not the frontend.
        // We often store this in configuration.
        // For now assuming these properties added to AuthenticationOptions or separate IdPOptions.
        
        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = _authOptions.ClientId,
            ClientSecret = _authOptions.ClientSecret,
            Scope = _authOptions.Scope ?? ""
        }, cancellationToken);

        if (tokenResponse.IsError)
        {
            throw new Exception($"Failed to retrieve M2M access token: {tokenResponse.Error}");
        }

        // 3. Cache Token (minus a buffer of 60 seconds)
        var expiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn).Subtract(TimeSpan.FromSeconds(60));
        _cache.Set(CacheKey, tokenResponse.AccessToken, expiration);

        return tokenResponse.AccessToken;
    }
}
