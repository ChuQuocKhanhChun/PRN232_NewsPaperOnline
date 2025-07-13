using PRN232_FinalProject.Models;

namespace PRN232_FinalProject.Repository.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId);
        Task<Comment> CreateAsync(Comment comment);
    }
        
}
