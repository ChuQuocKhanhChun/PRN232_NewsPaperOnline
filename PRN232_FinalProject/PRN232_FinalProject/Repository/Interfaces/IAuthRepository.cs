using PRN232_FinalProject.DTO;
using System.Security.Claims;

namespace PRN232_FinalProject.Repository.Interfaces
{
    public interface IAuthRepository
    {
        Task<(bool Success, IEnumerable<string>? Errors)> RegisterAsync(RegisterDto dto);
        Task<(string? Token, DateTime? Expiration, string? ErrorMessage)> LoginAsync(LoginDto dto);
        Task<UserProfileDto?> GetProfileAsync(string email);
       
    }
}
