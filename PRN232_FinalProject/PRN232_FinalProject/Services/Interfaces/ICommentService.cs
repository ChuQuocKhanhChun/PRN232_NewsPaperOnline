using PRN232_FinalProject.DTO;

namespace PRN232_FinalProject.Services.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetByArticleIdAsync(int articleId);
        Task<CommentDto> CreateAsync(CommentDto dto);
    }
}
