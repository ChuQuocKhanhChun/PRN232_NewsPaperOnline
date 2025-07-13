using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Repository.Interfaces;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var (token, _, errorMessage) = await _authRepository.LoginAsync(dto);
            if (token == null) throw new Exception(errorMessage ?? "Login failed");
            return token;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var (success, _) = await _authRepository.RegisterAsync(dto);
            return success;
        }

        public async Task<UserProfileDto?> GetProfileAsync(string email)
        {
            var result = await _authRepository.GetProfileAsync(email);
            if (result == null) return null;

            return new UserProfileDto
            {
                Email = result.Email,
                FullName = result.FullName,
                Roles = result.Roles
            };
        }
    }
}
