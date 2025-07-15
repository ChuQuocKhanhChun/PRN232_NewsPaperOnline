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
        public async Task<List<TagDto>> GetAllTagsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TagDto>>("https://localhost:7083/api/tags");
        }

        // Filter articles client-side (if API does not support filtering)
        public async Task<List<ArticleDto>> GetFilteredArticlesAsync(string search,int category, int tag, DateTime? publishDate)
        {
            var articles = await GetArticlesAsync();
            var tags = await GetTagByIdAsync(tag);

            if (!string.IsNullOrEmpty(search))
                articles = articles.Where(a => a.Title != null && a.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            if(category != 0)
                articles = articles.Where(a => a.CategoryId == category).ToList();
            if (tag!=0)
                articles = articles.Where(a => a is { TagNames: not null } && a.TagNames.Contains( tags.Name)).ToList();

            if (publishDate.HasValue)
                // Fix for CS1061: 'DateTime?' does not contain a definition for 'Date'.
                // The issue occurs because 'DateTime?' is a nullable type, and you need to access the 'Date' property of the underlying 'DateTime' value.
                // Use the Value property of the nullable type to access the underlying 'DateTime' and then access its 'Date' property.

                if (publishDate.HasValue)
                    articles = articles.Where(a => a.CreatedAt.HasValue && a.CreatedAt.Value.Date == publishDate.Value.Date).ToList();

            return articles;
        }

        // Get article by id
        public async Task<ArticleDto> GetArticleByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ArticleDto>($"https://localhost:7083/api/articles/{id}");
        }
        public async Task<TagDto> GetTagByIdAsync(int id)
        {
            if(id == 0)
            {
                return null;
            }
            return await _httpClient.GetFromJsonAsync<TagDto>($"https://localhost:7083/api/tags/{id}");
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
        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<CategoryDto>>("https://localhost:7083/api/Category");
        }
    }
}