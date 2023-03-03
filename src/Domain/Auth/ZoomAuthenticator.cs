using RestSharp;
using RestSharp.Authenticators;
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

        public ZoomAuthenticator(string baseUrl, Options options) : base("")
        {
            _baseUrl = baseUrl;
            _accountId = options.AccountId;
            _clientId = options.ClientId;
            _clientSecret = options.ClientSecret;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        async Task<string> GetToken()
        {
            var options = new RestClientOptions(_baseUrl);
            using var client = new RestClient(options)
            {
                Authenticator = new HttpBasicAuthenticator(_clientId, _clientSecret)
            };

            var request = new RestRequest("oauth/token", Method.Post)
                .AddParameter("grant_type", "account_credentials")
                .AddParameter("account_id", _accountId);
            var response = await client.PostAsync<TokenResponse>(request);
            return $"{response!.TokenType} {response!.AccessToken}";
        }
    }
}
