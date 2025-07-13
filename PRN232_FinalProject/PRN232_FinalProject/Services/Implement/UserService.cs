using AutoMapper;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Repository.Interfaces;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllAsync() => _mapper.Map<IEnumerable<UserProfileDto>>(await _repo.GetAllAsync());

        public async Task<UserProfileDto?> GetByIdAsync(string id) => _mapper.Map<UserProfileDto>(await _repo.GetByIdAsync(id));

        public async Task<bool> DeleteAsync(string id) => await _repo.DeleteAsync(id);
    }
}
