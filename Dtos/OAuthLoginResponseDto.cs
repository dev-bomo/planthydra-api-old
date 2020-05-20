using Newtonsoft.Json;

namespace api.Dtos
{
    /// <summary>
    /// OAuth login response DTO
    /// </summary>
    public class OAuthLoginResponseDto
    {
        /// <summary>
        /// The access token
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}