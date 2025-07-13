using PRN232_FinalProject.Models;

namespace PRN232_FinalProject.Repository.Interfaces
{
    public interface IArticleRepository
    {
        Task<IEnumerable<Article>> GetAllAsync();
        Task<Article?> GetByIdAsync(int id);
        Task<Article> CreateAsync(Article article);

        Task<Article?> UpdateAsync(int id, Article article);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Article>> SearchAsync(string keyword);
        Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Article>> GetRecentAsync(int count);
        Task<IEnumerable<Article>> GetByAuthorEmailAsync(string email);
        Task<int> CountAsync();
        Task<object> UpdateAsync(Article article);
    }

}
