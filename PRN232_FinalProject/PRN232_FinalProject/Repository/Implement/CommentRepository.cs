using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Interfaces;

namespace PRN232_FinalProject.Repository.Implement
{
    public class CommentRepository : ICommentRepository
    {
        private readonly Prn232FinalProjectContext _context;

        public CommentRepository(Prn232FinalProjectContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId)
        {
            return await _context.Comments.Where(c => c.ArticleId == articleId).ToListAsync();
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
    }
}
