using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Interfaces;

namespace PRN232_FinalProject.Repository.Implement
{
    public class TagRepository : ITagRepository
    {
        private readonly Prn232FinalProjectContext _context;

        public TagRepository(Prn232FinalProjectContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tag>> GetAllAsync() => await _context.Tags.ToListAsync();

        public async Task<Tag?> GetByIdAsync(int id) => await _context.Tags.FindAsync(id);

        public async Task<Tag> CreateAsync(Tag tag)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return false;
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
