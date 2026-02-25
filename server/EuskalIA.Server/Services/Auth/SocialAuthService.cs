using EuskalIA.Server.Models;
using Google.Apis.Auth;
using Newtonsoft.Json;
using System.Net.Http;

namespace EuskalIA.Server.Services.Auth
{
    public class SocialAuthService : ISocialAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SocialAuthService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<User?> ValidateGoogleTokenAsync(string token)
        {
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
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<User?> ValidateFacebookTokenAsync(string token)
        {
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
            catch (Exception)
            {
                return null;
            }
        }
    }
}
