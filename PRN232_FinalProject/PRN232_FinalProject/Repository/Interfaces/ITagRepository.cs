using PRN232_FinalProject.Models;

namespace PRN232_FinalProject.Repository.Interfaces
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetAllAsync();
        Task<Tag> GetByIdAsync(int id);
        Task<Tag> CreateAsync(Tag tag);
        Task<bool> DeleteAsync(int id);
        Task<Tag> UpdateAsync(Tag tag);
    }
}
