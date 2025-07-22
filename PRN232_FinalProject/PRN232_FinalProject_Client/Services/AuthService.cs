using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PRN232_FinalProject_Client.DTO;

namespace PRN232_FinalProject_Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

            public async Task<TokenResponse> LoginAsync(LoginDto dto)
            {
                var response = await _httpClient.PostAsJsonAsync("https://localhost:7083/api/auth/login", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Status: {response.StatusCode}, Message: {errorContent}");
                }

                return await response.Content.ReadFromJsonAsync<TokenResponse>();
            }


        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7083/api/auth/register", dto);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Đăng ký thất bại: " + content); // log lỗi API
            return false;
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7083/api/auth/forgot-password", dto);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Đã gửi liên kết khôi phục mật khẩu đến email.");
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
       

    }
} 