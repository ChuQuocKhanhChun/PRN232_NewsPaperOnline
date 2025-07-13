using AutoMapper;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Services.Interfaces;

namespace PRN232_FinalProject.Services.Implement
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repo;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CommentDto>> GetByArticleIdAsync(int articleId) => _mapper.Map<IEnumerable<CommentDto>>(await _repo.GetByArticleIdAsync(articleId));

        public async Task<CommentDto> CreateAsync(CommentDto dto)
        {
            var comment = _mapper.Map<Comment>(dto);
            return _mapper.Map<CommentDto>(await _repo.CreateAsync(comment));
        }
    }

}
