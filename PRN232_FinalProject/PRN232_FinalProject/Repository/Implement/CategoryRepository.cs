using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Interfaces;

namespace PRN232_FinalProject.Repository.Implement
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly Prn232FinalProjectContext _context;

        public CategoryRepository(Prn232FinalProjectContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync() => await _context.Categories.ToListAsync();

        public async Task<Category?> GetByIdAsync(int id) => await _context.Categories.FindAsync(id);

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
