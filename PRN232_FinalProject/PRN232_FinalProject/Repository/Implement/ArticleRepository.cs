using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Interfaces;
using System;

namespace PRN232_FinalProject.Repository.Implement
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly Prn232FinalProjectContext _context;

        public ArticleRepository(Prn232FinalProjectContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ArticleDto>> GetAllAsync() => (IEnumerable<ArticleDto>)await _context.Articles
    .Include(a => a.Tags)
    .Select(a => new ArticleDto
    {
        ArticleID = a.ArticleId,
        Title = a.Title,
        Content = a.Content,
        Status = a.Status,
        CreatedAt = a.CreatedAt,
        CategoryId = a.CategoryId,
        CategoryName = a.Category.Name,
        TagIds = a.Tags.Select(t => t.TagId).ToList(),
        TagNames = a.Tags.Select(t => t.Name).ToList(),
    }).ToListAsync()
    ;


        public async Task<Article?> GetByIdAsync(int id) => await _context.Articles.Include(a => a.Category).Include(a => a.Tags).Include(a => a.Author).FirstOrDefaultAsync(x => x.ArticleId == id);

        public async Task<Article> CreateAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> UpdateAsync(int id, Article article)
        {
            var existingArticle = await _context.Articles.FindAsync(id);
            if (existingArticle == null) return null;
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return false;
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Article>> GetByAuthorEmailAsync(string email)
        {
            return await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Tags)
                .Include(a => a.Author)
                .Where(a => a.Author != null && a.Author.Email == email)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> SearchAsync(string keyword)
        {
            return await _context.Articles
                .Where(a => a.Title.Contains(keyword) || a.Content.Contains(keyword))
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Articles.Where(a => a.CategoryId == categoryId).ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetRecentAsync(int count)
        {
            return await _context.Articles.OrderByDescending(a => a.CreatedAt).Take(count).ToListAsync();
        }

        public async Task<int> CountAsync() => await _context.Articles.CountAsync();

        public Task<object> UpdateAsync(Article article)
        {
            throw new NotImplementedException();
        }
    }


}
