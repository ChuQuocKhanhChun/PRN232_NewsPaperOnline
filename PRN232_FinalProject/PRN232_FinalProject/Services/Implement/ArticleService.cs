using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Implement;
using PRN232_FinalProject.Repository.Interfaces;
using PRN232_FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<ArticleDto>> GetAllAsync() => _mapper.Map<IEnumerable<ArticleDto>>(await _repo.GetAllAsync());

        public async Task<ArticleDto?> GetByIdAsync(int id) => _mapper.Map<ArticleDto>(await _repo.GetByIdAsync(id));

        public async Task<ArticleDto> CreateAsync(ArticleDto dto)
        {
            var article = _mapper.Map<Article>(dto);
            return _mapper.Map<ArticleDto>(await _repo.CreateAsync(article));
        }

        public async Task<ArticleDto?> UpdateAsync(int id, ArticleDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            _mapper.Map(dto, entity);
            return _mapper.Map<ArticleDto>(await _repo.UpdateAsync(entity));
        }

        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        public async Task<IEnumerable<ArticleDto>> SearchAsync(string keyword) => _mapper.Map<IEnumerable<ArticleDto>>(await _repo.SearchAsync(keyword));

        public async Task<IEnumerable<ArticleDto>> GetByCategoryAsync(int categoryId) => _mapper.Map<IEnumerable<ArticleDto>>(await _repo.GetByCategoryAsync(categoryId));

        public async Task<IEnumerable<ArticleDto>> GetRecentAsync(int count) => _mapper.Map<IEnumerable<ArticleDto>>(await _repo.GetRecentAsync(count));

        public async Task<ArticleDto?> UpdateStatusAsync(int id, string status)
        {
            var article = await _repo.GetByIdAsync(id);
            if (article == null) return null;
            article.Status = status;
            return _mapper.Map<ArticleDto>(await _repo.UpdateAsync(article));
        }

        public async Task<int> GetCountAsync() => await _repo.CountAsync();

        public async Task<IEnumerable<ArticleDto>> GetByAuthorEmailAsync(string email)
        {
            return (IEnumerable<ArticleDto>)await _repo.GetByAuthorEmailAsync(email);
        }
        public async Task<int> GetLikeCountAsync(int articleId)
    => await _repo.GetLikeCountAsync(articleId);

        public async Task<bool> IsLikedAsync(int articleId, string userId)
            => await _repo.IsLikedAsync(articleId, userId);

        public async Task LikeAsync(int articleId, string userId)
            => await _repo.LikeAsync(articleId, userId);

        public async Task UnlikeAsync(int articleId, string userId)
            => await _repo.UnlikeAsync(articleId, userId);

    }

}
