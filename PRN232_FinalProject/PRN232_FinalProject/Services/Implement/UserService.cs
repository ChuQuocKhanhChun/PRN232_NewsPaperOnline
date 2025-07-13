using AutoMapper;

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

        public async Task<IEnumerable<UserDto>> GetAllAsync() => _mapper.Map<IEnumerable<UserDto>>(await _repo.GetAllAsync());

        public async Task<UserDto?> GetByIdAsync(string id) => _mapper.Map<UserDto>(await _repo.GetByIdAsync(id));

        public async Task<bool> DeleteAsync(string id) => await _repo.DeleteAsync(id);
    }
}
