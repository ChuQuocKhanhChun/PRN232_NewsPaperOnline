using PRN232_FinalProject.DTO;

namespace PRN232_FinalProject.Services.Interfaces
{
    public interface IArticleService
    {
        Task<IEnumerable<ArticleDto>> GetAllAsync();
        Task<ArticleDto?> GetByIdAsync(int id);
        Task<ArticleDto> CreateAsync(ArticleDto dto);
        Task<ArticleDto?> UpdateAsync(int id, ArticleDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ArticleDto>> SearchAsync(string keyword);
        Task<IEnumerable<ArticleDto>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<ArticleDto>> GetRecentAsync(int count);
        Task<ArticleDto?> UpdateStatusAsync(int id, string status);
        Task<int> GetCountAsync();
    }

}
