using PRN232_FinalProject.DTO;

namespace PRN232_FinalProject.Services.Interfaces
{
    public interface ITagService
    {
        Task<TagDto> CreateAsync(TagDto dto);
        Task<IEnumerable<TagDto>> GetAllAsync();
        Task<bool> DeleteAsync(int id);
    }
}
