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
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TokenResponse>();
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7083/api/auth/register", dto);
            return response.IsSuccessStatusCode;
        }
    }
} 