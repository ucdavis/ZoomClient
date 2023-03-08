using Microsoft.Extensions.Caching.Memory;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace ZoomClient.Domain.Auth
{
    /// <summary>
    /// OAuth2 Authenticator for Zoom
    /// </summary>
    /// <remarks>https://restsharp.dev/usage.html#authenticator</remarks>
    internal class ZoomAuthenticator : AuthenticatorBase
    {
        readonly string _baseUrl;
        readonly string _accountId;
        readonly string _clientId;
        readonly string _clientSecret;
        private readonly IMemoryCache _memoryCache;
        private const string CACHEKEY = "authtoken";

        public ZoomAuthenticator(string baseUrl, Options options, IMemoryCache cache) : base("")
        {
            _baseUrl = baseUrl;
            _accountId = options.AccountId;
            _clientId = options.ClientId;
            _clientSecret = options.ClientSecret;
            _memoryCache = cache;
        }

        protected override async ValueTask<RestSharp.Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        async Task<string> GetToken()
        {
            if (_memoryCache.TryGetValue<string>(CACHEKEY, out var token))
            {
                return token;
            }

            var options = new RestClientOptions(_baseUrl);
            using var client = new RestClient(options)
            {
                Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret)
            };

            var request = new RestRequest("oauth/token", Method.Post)
                .AddParameter("grant_type", "account_credentials")
                .AddParameter("account_id", _accountId);
            var response = await client.PostAsync<TokenResponse>(request);
            token = $"{response!.TokenType} {response!.AccessToken}";

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(55))
                .SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _memoryCache.Set(CACHEKEY, token);

            return token;
        }
    }
}
