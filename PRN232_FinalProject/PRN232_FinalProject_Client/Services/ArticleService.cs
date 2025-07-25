using PRN232_FinalProject.Controllers;
using PRN232_FinalProject_Client.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PRN232_FinalProject_Client.Services
{
    public class ArticleService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7083/api/Comment";
        private const string BaseArticleUrl = "https://localhost:7083/api/articles";
        public ArticleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        // Phương thức để lấy các bình luận theo cấu trúc cây
        public async Task<int> GetLikeCountAsync(int articleId)
        {
            var response = await _httpClient.GetAsync($"{BaseArticleUrl}/{articleId}/like-count");
            if (response.IsSuccessStatusCode)
            {
                var countString = await response.Content.ReadAsStringAsync();
                return int.TryParse(countString, out int count) ? count : 0;
            }
            return 0;
        }


        public async Task<bool> IsLikedByUserAsync(int articleId, string userId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseArticleUrl}/{articleId}/is-liked/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result.Trim().ToLower() == "true";
            }
            return false;
        }


        public async Task<bool> LikeArticleAsync(int articleId, string userId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseArticleUrl}/{articleId}/like/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> UnlikeArticleAsync(int articleId, string userId, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseArticleUrl}/{articleId}/unlike/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CommentTreeDto>> GetCommentsTreeByArticleAsync(int articleId)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/tree?articleId={articleId}");
            response.EnsureSuccessStatusCode(); // Đảm bảo request thành công (2xx status)
            return await response.Content.ReadFromJsonAsync<List<CommentTreeDto>>();
        }

        // Phương thức để tạo bình luận
        public async Task<bool> PostCommentAsync(DTO.CommentDto commentDto, string? token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            request.Content = JsonContent.Create(commentDto);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // Phương thức để cập nhật bình luận
        public async Task<bool> UpdateCommentAsync(int commentId, DTO.CommentUpdateDto commentDto, string? token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/{commentId}");
            request.Content = JsonContent.Create(commentDto);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        // Phương thức để xóa bình luận (xóa mềm)
        public async Task<bool> DeleteCommentAsync(int commentId, string? token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{commentId}");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
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