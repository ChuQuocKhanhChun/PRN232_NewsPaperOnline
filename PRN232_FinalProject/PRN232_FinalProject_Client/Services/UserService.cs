using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.DTO;
using System.Net.Http.Headers;

namespace PRN232_FinalProject_Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<HttpResponseMessage> CallUpdateProfileApiAsync(string userId, MultipartFormDataContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://localhost:7083/api/auth/update-profile");
            request.Content = content;

            // Thêm JWT nếu cần
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _httpClient.SendAsync(request);
        }




        public async Task<UserProfileDto> GetUserProfileAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token is null or empty.");
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7083/api/auth/profile");
                Console.WriteLine($"API Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var userProfile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                    if (userProfile == null)
                    {
                        Console.WriteLine("API returned success but userProfile is null (deserialization issue).");
                    }
                    return userProfile;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {error}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return null;
            }
        }

    }
}
