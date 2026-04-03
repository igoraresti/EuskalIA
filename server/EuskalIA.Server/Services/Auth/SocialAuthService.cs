using EuskalIA.Server.Models;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth;
using Newtonsoft.Json;
using System.Net.Http;

namespace EuskalIA.Server.Services.Auth
{
    /// <summary>
    /// Implementation of <see cref="ISocialAuthService"/> that handles token validation with Google and Facebook APIs.
    /// Manages HTTP communication and configuration retrieval for social app credentials.
    /// </summary>
    public class SocialAuthService : ISocialAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SocialAuthService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialAuthService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration provider.</param>
        /// <param name="httpClient">The HTTP client for external API requests.</param>
        /// <param name="logger">The service logger.</param>
        public SocialAuthService(IConfiguration configuration, HttpClient httpClient, ILogger<SocialAuthService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Validates a Google token (Access Token or ID Token) and returns the corresponding user profile.
        /// Supports both "ya29." style access tokens and standard JWT ID tokens.
        /// </summary>
        /// <param name="token">The Google token string.</param>
        /// <returns>A <see cref="User"/> object populated with Google identity data; otherwise, null.</returns>
        public async Task<User?> ValidateGoogleTokenAsync(string token)
        {
            _logger.LogInformation("Validating Google token.");
            try
            {
                // Google Access Tokens usually start with "ya29."
                // If it's an access token, we fetch the user info from the Google API
                if (token.StartsWith("ya29."))
                {
                    var userInfoUrl = $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={token}";
                    var userInfoResponse = await _httpClient.GetAsync(userInfoUrl);
                    if (!userInfoResponse.IsSuccessStatusCode) return null;

                    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                    dynamic userInfo = JsonConvert.DeserializeObject(userInfoContent)!;

                    return new User
                    {
                        Username = userInfo.email,
                        Email = userInfo.email,
                        Nickname = userInfo.name,
                        IsVerified = userInfo.email_verified == true || userInfo.email_verified == "true"
                    };
                }
                else
                {
                    // Fallback to ID Token validation
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                    };

                    var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

                    return new User
                    {
                        Username = payload.Email,
                        Email = payload.Email,
                        Nickname = payload.Name,
                        IsVerified = true // Google emails are verified
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token.");
                return null;
            }
        }

        /// <summary>
        /// Validates a Facebook access token using the Facebook Graph API and returns user profile data.
        /// Performs a multi-step validation: first debugging the token, then fetching user fields.
        /// </summary>
        /// <param name="token">The Facebook access token string.</param>
        /// <returns>A <see cref="User"/> object populated with Facebook identity data; otherwise, null.</returns>
        public async Task<User?> ValidateFacebookTokenAsync(string token)
        {
            _logger.LogInformation("Validating Facebook token.");
            try
            {
                // 1. Verify token with Facebook Graph API
                var appAccessToken = $"{_configuration["Authentication:Facebook:AppId"]}|{_configuration["Authentication:Facebook:AppSecret"]}";
                var debugTokenUrl = $"https://graph.facebook.com/debug_token?input_token={token}&access_token={appAccessToken}";

                var debugResponse = await _httpClient.GetAsync(debugTokenUrl);
                if (!debugResponse.IsSuccessStatusCode) return null;

                var debugContent = await debugResponse.Content.ReadAsStringAsync();
                dynamic debugData = JsonConvert.DeserializeObject(debugContent)!;

                if (debugData.data.is_valid != true) return null;

                // 2. Get user info
                var userInfoUrl = $"https://graph.facebook.com/me?fields=id,name,email&access_token={token}";
                var userInfoResponse = await _httpClient.GetAsync(userInfoUrl);
                if (!userInfoResponse.IsSuccessStatusCode) return null;

                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                dynamic userInfo = JsonConvert.DeserializeObject(userInfoContent)!;

                string email = userInfo.email ?? $"{userInfo.id}@facebook.com";

                return new User
                {
                    Username = email,
                    Email = email,
                    Nickname = (string)userInfo.name,
                    IsVerified = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Facebook token.");
                return null;
            }
        }
    }
}
