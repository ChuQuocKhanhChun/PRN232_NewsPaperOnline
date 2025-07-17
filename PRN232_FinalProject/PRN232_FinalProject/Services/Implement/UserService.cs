using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Identity;
using PRN232_FinalProject.Repository.Implement;
using PRN232_FinalProject.Repository.Interfaces;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(IUserRepository repo, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllAsync() => _mapper.Map<IEnumerable<UserProfileDto>>(await _repo.GetAllAsync());

        public async Task<UserProfileDto?> GetByIdAsync(string id) => _mapper.Map<UserProfileDto>(await _repo.GetByIdAsync(id));

        public async Task<bool> DeleteAsync(string id) => await _repo.DeleteAsync(id);
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            return result.Succeeded;
        }
    }
}
