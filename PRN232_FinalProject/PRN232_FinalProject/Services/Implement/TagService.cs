using AutoMapper;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Interfaces;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Services.Implement
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _repo;
        private readonly IMapper _mapper;

        public TagService(ITagRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TagDto>> GetAllAsync() => _mapper.Map<IEnumerable<TagDto>>(await _repo.GetAllAsync());

        public async Task<TagDto?> GetByIdAsync(int id) => _mapper.Map<TagDto>(await _repo.GetByIdAsync(id));

        public async Task<TagDto> CreateAsync(TagDto dto)
        {
            var tag = _mapper.Map<Tag>(dto);
            return _mapper.Map<TagDto>(await _repo.CreateAsync(tag));
        }

        public async Task<TagDto?> UpdateAsync(int id, TagDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            _mapper.Map(dto, entity);
            return _mapper.Map<TagDto>(await _repo.UpdateAsync(entity));
        }

        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}
