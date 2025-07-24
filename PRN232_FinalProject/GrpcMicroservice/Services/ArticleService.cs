using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcArticleService;
using PRN232_FinalProject.Models;
using System.Collections.Concurrent;

namespace GrpcMicroservice.Services
{
    public class ArticleService : GrpcArticleService.ArticleService.ArticleServiceBase
    {
        private readonly Prn232FinalProjectContext _context;
        private readonly ILogger<ArticleService> _logger;

        public ArticleService(Prn232FinalProjectContext context, ILogger<ArticleService> logger) { 
        
            _context = context;
            _logger = logger;
        }

        public override async Task<ArticleResponse> CreateArticle(CreateArticleRequest request, ServerCallContext context)
        {
            var article = new Article
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = request.AuthorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Article created: {Id}", article.ArticleId);

            return new ArticleResponse
            {
                Id = article.ArticleId.ToString(),
                Title = article.Title,
                Content = article.Content,
                AuthorId = article.AuthorId,
                CreatedAt = article.CreatedAt.HasValue ? article.CreatedAt.Value.ToString("o") : string.Empty
            };
        }

        public override async Task<ArticleResponse> UpdateArticle(UpdateArticleRequest request, ServerCallContext context)
        {
            var article = await _context.Articles.FindAsync(request.Id);
            if (article == null)
            {
                return new ArticleResponse
                {
                    Id = request.Id,
                    Title = "",
                    Content = "",
                    AuthorId = "",
                    CreatedAt = ""
                };
            }

            article.Title = request.Title;
            article.Content = request.Content;

            await _context.SaveChangesAsync();


            return new ArticleResponse
            {
                Id = article.ArticleId.ToString(),
                Title = article.Title,
                Content = article.Content,
                AuthorId = article.AuthorId,
                CreatedAt = article.CreatedAt.HasValue ? article.CreatedAt.Value.ToString("o") : string.Empty
            };
        }

        public override async Task<DeleteArticleResult> DeleteArticle(DeleteArticleRequest request, ServerCallContext context)
        {
            var article = await _context.Articles.FindAsync(request.Id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Article deleted: {Id}", request.Id);
                return new DeleteArticleResult { Success = true };
            }
            return new DeleteArticleResult { Success = false };
        }
    }

}
