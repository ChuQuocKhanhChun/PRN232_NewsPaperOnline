using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PRN232_FinalProject_Client.DTO;

namespace PRN232_FinalProject_Client.Services
{
    public class ArticleService
    {
        private readonly HttpClient _httpClient;

        public ArticleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ArticleDto>> GetArticlesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ArticleDto>>("https://localhost:7083/api/articles");
        }

        // Get all tags (assuming your API provides this endpoint)
        public async Task<List<string>> GetAllTagsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:7083/api/tags");
        }

        // Filter articles client-side (if API does not support filtering)
        public async Task<List<ArticleDto>> GetFilteredArticlesAsync(string search, string tag, DateTime? publishDate)
        {
            var articles = await GetArticlesAsync();

            if (!string.IsNullOrEmpty(search))
                articles = articles.Where(a => a.Title != null && a.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(tag))
                articles = articles.Where(a => a is { TagNames: not null } && a.TagNames.Contains(tag)).ToList();

            if (publishDate.HasValue)
                articles = articles.Where(a => a.CreatedAt.Date == publishDate.Value.Date).ToList();

            return articles;
        }

        // Get article by id
        public async Task<ArticleDto> GetArticleByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ArticleDto>($"https://localhost:7083/api/articles/{id}");
        }

        // Create article with authentication token
        public async Task<bool> CreateArticleAsync(ArticleCreateDto dto, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7083/api/articles")
            {
                Content = JsonContent.Create(dto)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}