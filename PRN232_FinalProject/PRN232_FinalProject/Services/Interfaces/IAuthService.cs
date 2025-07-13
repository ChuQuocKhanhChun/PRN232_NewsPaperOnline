using PRN232_FinalProject.DTO;

namespace PRN232_FinalProject.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDto dto);
        Task<bool> RegisterAsync(RegisterDto dto);
        Task<UserProfileDto> GetProfileAsync(string email);
    }
}
