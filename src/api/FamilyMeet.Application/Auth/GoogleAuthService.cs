using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using FamilyMeet.Domain.Users;
using FamilyMeet.Domain.Settings;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FamilyMeet.Application.Auth
{
    public class GoogleAuthService : ITransientDependency
    {
        private readonly IConfiguration _configuration;
        private readonly ISettingManager _settingManager;
        private readonly HttpClient _httpClient;

        public GoogleAuthService(
            IConfiguration configuration,
            ISettingManager settingManager,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _settingManager = settingManager;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken)
        {
            var clientId = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Authentication.GoogleClientId)
                          ?? _configuration["Authentication:Google:ClientId"];

            if (string.IsNullOrEmpty(clientId))
            {
                throw new UserFriendlyException("Google Client ID is not configured.");
            }

            // Validate token with Google
            var validationUrl = $"https://www.googleapis.com/oauth2/v1/tokeninfo?id_token={idToken}";

            var response = await _httpClient.GetAsync(validationUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (tokenInfo?.Audience != clientId)
            {
                throw new UserFriendlyException("Invalid Google token.");
            }

            // Get user info
            var userInfo = await GetUserInfoAsync(tokenInfo.Subject);
            return userInfo;
        }

        public async Task<GoogleUserInfo> GetUserInfoAsync(string googleId)
        {
            var userInfoUrl = $"https://www.googleapis.com/oauth2/v2/userinfo";

            var response = await _httpClient.GetAsync(userInfoUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (userInfo == null)
            {
                throw new UserFriendlyException("Failed to get Google user information.");
            }

            return userInfo;
        }

        public async Task<string> GetAuthUrlAsync(string redirectUri, string state = null)
        {
            var clientId = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Authentication.GoogleClientId)
                          ?? _configuration["Authentication:Google:ClientId"];

            var scopes = new List<string>
            {
                "openid",
                "profile",
                "email"
            };

            var url = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                     $"client_id={clientId}&" +
                     $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                     $"response_type=code&" +
                     $"scope={string.Join(" ", scopes)}";

            if (!string.IsNullOrEmpty(state))
            {
                url += $"&state={Uri.EscapeDataString(state)}";
            }

            return url;
        }

        public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
        {
            var clientId = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Authentication.GoogleClientId)
                          ?? _configuration["Authentication:Google:ClientId"];

            var clientSecret = await _settingManager.GetOrNullAsync(FamilyMeetSettings.Authentication.GoogleClientSecret)
                              ?? _configuration["Authentication:Google:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new UserFriendlyException("Google Client ID or Client Secret is not configured.");
            }

            var tokenUrl = "https://oauth2.googleapis.com/token";

            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(tokenUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (tokenResponse == null)
            {
                throw new UserFriendlyException("Failed to exchange authorization code for token.");
            }

            return tokenResponse;
        }

        public async Task<GoogleUserInfo> GetUserInfoFromAccessTokenAsync(string accessToken)
        {
            var userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(userInfoUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            if (userInfo == null)
            {
                throw new UserFriendlyException("Failed to get Google user information.");
            }

            return userInfo;
        }
    }

    public class GoogleTokenInfo
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime IssuedAt { get; set; }
    }

    public class GoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string IdToken { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool VerifiedEmail { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;

        public string GetFirstName()
        {
            return GivenName ?? Name?.Split(' ').FirstOrDefault() ?? string.Empty;
        }

        public string GetLastName()
        {
            return FamilyName ?? Name?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty;
        }
    }
}
