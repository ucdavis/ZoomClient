using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<Zoom> _logger;
        private const string CACHEKEY = "authtoken";

        public ZoomAuthenticator(string baseUrl, Options options, IMemoryCache cache, ILogger<Zoom> logger) : base("")
        {
            _baseUrl = baseUrl;
            _accountId = options.AccountId;
            _clientId = options.ClientId;
            _clientSecret = options.ClientSecret;
            _memoryCache = cache;
            _logger = logger;
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
                _logger.LogInformation($"ZoomAuthenticator.GetToken found token in cache.");
                return token;
            }

            var options = new RestClientOptions(_baseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret)
            };
            using var client = new RestClient(options);

            var request = new RestRequest("oauth/token", Method.Post)
                .AddParameter("grant_type", "account_credentials")
                .AddParameter("account_id", _accountId);
            var response = await client.PostAsync<TokenResponse>(request);
            token = $"{response!.TokenType} {response!.AccessToken}";

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(55))
                .SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _memoryCache.Set(CACHEKEY, token, cacheEntryOptions);
            _logger.LogInformation($"ZoomAuthenticator.GetToken added token to cache.");

            return token;
        }
    }
}
