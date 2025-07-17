using PRN232_FinalProject.DTO;

namespace PRN232_FinalProject.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserProfileDto>> GetAllAsync();
         Task<UserProfileDto?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
