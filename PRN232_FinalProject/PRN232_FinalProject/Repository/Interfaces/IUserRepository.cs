using Microsoft.AspNetCore.Identity;
using PRN232_FinalProject.Identity;

namespace PRN232_FinalProject.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id);
    }
}
