using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Article>> GetAllAsync() =>
            await _context.Articles.ToListAsync();

        public async Task<Article?> GetByIdAsync(int id) =>
            await _context.Articles.FindAsync(id);

        public async Task<Article> CreateAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }
        // Cập nhật bài viết, thiết lập UpdatedAt
        public async Task<Article?> UpdateAsync(int id, Article article)
        {
            var existing = await _context.Articles.FindAsync(id);
            if (existing == null || existing.IsDeleted.GetValueOrDefault()) return null;

            existing.Title = article.Title;
            existing.Content = article.Content;
            existing.ImageUrl = article.ImageUrl;
            existing.PublishedDate = article.PublishedDate;
            existing.Status = article.Status;
            existing.Views = article.Views;
            existing.AuthorId = article.AuthorId;
            existing.CategoryId = article.CategoryId;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        // Xóa mềm bài viết bằng cách đặt IsDeleted = true
        public async Task<bool> DeleteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null || article.IsDeleted.GetValueOrDefault()) return false;

            article.IsDeleted = true;
            article.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        // Tìm kiếm bài viết chưa bị xóa theo tiêu đề hoặc nội dung
        public async Task<IEnumerable<Article>> SearchAsync(string keyword)
        {
            return await _context.Articles
                .Where(a => a.IsDeleted != true &&
                            (a.Title.Contains(keyword) || a.Content.Contains(keyword)))
                .Include(a => a.Author)
                .Include(a => a.Category)
                .ToListAsync();
        }

        // Lấy bài viết theo danh mục, chưa bị xóa
        public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Articles
                .Where(a => a.IsDeleted != true && a.CategoryId == categoryId)
                .Include(a => a.Author)
                .Include(a => a.Category)
                .ToListAsync();
        }

        // Lấy các bài viết gần đây nhất theo CreatedAt
        public async Task<IEnumerable<Article>> GetRecentAsync(int count)
        {
            return await _context.Articles
                .Where(a => a.IsDeleted != true)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .Include(a => a.Author)
                .Include(a => a.Category)
                .ToListAsync();
        }

        // Cập nhật trạng thái bài viết
        public async Task<Article?> UpdateStatusAsync(int id, string status)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null || article.IsDeleted.GetValueOrDefault()) return null;

            article.Status = status;
            article.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return article;
        }

        // Đếm số bài viết chưa bị xóa
        public async Task<int> GetCountAsync()
        {
            return await _context.Articles
                .Where(a => a.IsDeleted != true)
                .CountAsync();
        }
    }

}
