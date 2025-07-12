using AutoMapper;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Interfaces;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Services.Implement
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _repo;
        private readonly IMapper _mapper;

        public ArticleService(IArticleRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ArticleDto>> GetAllAsync()
        {
            var articles = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<ArticleDto>>(articles);
        }

        public async Task<ArticleDto?> GetByIdAsync(int id)
        {
            var article = await _repo.GetByIdAsync(id);
            return article != null ? _mapper.Map<ArticleDto>(article) : null;
        }

        public async Task<ArticleDto> CreateAsync(ArticleDto dto)
        {
            var article = _mapper.Map<Article>(dto);
            var created = await _repo.CreateAsync(article);
            return _mapper.Map<ArticleDto>(created);
        }
        public async Task<ArticleDto?> UpdateAsync(int id, ArticleDto dto)
        {
            var article = _mapper.Map<Article>(dto);
            var updated = await _repo.UpdateAsync(id, article);
            return updated != null ? _mapper.Map<ArticleDto>(updated) : null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<ArticleDto>> SearchAsync(string keyword)
        {
            var articles = await _repo.SearchAsync(keyword);
            return _mapper.Map<IEnumerable<ArticleDto>>(articles);
        }

        public async Task<IEnumerable<ArticleDto>> GetByCategoryAsync(int categoryId)
        {
            var articles = await _repo.GetByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ArticleDto>>(articles);
        }

        public async Task<IEnumerable<ArticleDto>> GetRecentAsync(int count)
        {
            var articles = await _repo.GetRecentAsync(count);
            return _mapper.Map<IEnumerable<ArticleDto>>(articles);
        }

        public async Task<ArticleDto?> UpdateStatusAsync(int id, string status)
        {
            var updated = await _repo.UpdateStatusAsync(id, status);
            return updated != null ? _mapper.Map<ArticleDto>(updated) : null;
        }

        public async Task<int> GetCountAsync()
        {
            return await _repo.GetCountAsync();
        }
    }

}
