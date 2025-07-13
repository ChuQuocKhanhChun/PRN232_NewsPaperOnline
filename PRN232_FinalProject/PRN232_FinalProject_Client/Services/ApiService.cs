namespace PRN232_FinalProject_Client.Services
{
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using Newtonsoft.Json;
    using PRN232_FinalProject_Client.DTO;

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddJwtToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("token").GetString();
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AddJwtToken();
            var response = await _httpClient.GetAsync(endpoint);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>() : default;
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            AddJwtToken();
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>() : default;
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            AddJwtToken();
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<T>() : default;
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            AddJwtToken();
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

    }

}
