using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.Concurrent;

namespace GrpcMicroservice.Services
{
    public class ArticleService : ArticleService.ArticleServiceBase
    {
        // In-memory DB giả
        private static readonly ConcurrentDictionary<string, Article> _articles = new();

        public override Task<ArticleList> GetAll(Empty request, ServerCallContext context)
        {
            var list = new ArticleList();
            list.Articles.AddRange(_articles.Values);
            return Task.FromResult(list);
        }

        public override Task<Article> GetById(ArticleIdRequest request, ServerCallContext context)
        {
            _articles.TryGetValue(request.Id, out var article);
            return Task.FromResult(article ?? new Article());
        }

        public override Task<Article> Create(Article request, ServerCallContext context)
        {
            var id = Guid.NewGuid().ToString();
            var newArticle = request with { Id = id };
            _articles[id] = newArticle;
            return Task.FromResult(newArticle);
        }

        public override Task<Article> Update(Article request, ServerCallContext context)
        {
            if (_articles.ContainsKey(request.Id))
            {
                _articles[request.Id] = request;
                return Task.FromResult(request);
            }
            throw new RpcException(new Status(StatusCode.NotFound, "Article not found"));
        }

        public override Task<Empty> Delete(ArticleIdRequest request, ServerCallContext context)
        {
            _articles.TryRemove(request.Id, out _);
            return Task.FromResult(new Empty());
        }
    }

}
