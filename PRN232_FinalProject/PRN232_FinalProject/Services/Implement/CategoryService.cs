using AutoMapper;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync() => _mapper.Map<IEnumerable<CategoryDto>>(await _repo.GetAllAsync());

        public async Task<CategoryDto?> GetByIdAsync(int id) => _mapper.Map<CategoryDto>(await _repo.GetByIdAsync(id));

        public async Task<CategoryDto> CreateAsync(CategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            return _mapper.Map<CategoryDto>(await _repo.CreateAsync(category));
        }

        public async Task<CategoryDto?> UpdateAsync(int id, CategoryDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            _mapper.Map(dto, entity);
            return _mapper.Map<CategoryDto>(await _repo.UpdateAsync(entity));
        }

        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}
