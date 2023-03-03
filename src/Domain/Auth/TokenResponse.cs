using System.Text.Json.Serialization;

namespace ZoomClient.Domain.Auth
{
    /// <summary>
    /// OAuth2 token response
    /// </summary>
    /// <remarks>https://restsharp.dev/usage.html#authenticator</remarks>
    internal class TokenResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}
