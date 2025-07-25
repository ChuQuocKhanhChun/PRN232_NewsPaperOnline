using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcArticleService;
using Microsoft.EntityFrameworkCore;
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
        public override async Task<TagList> GetAllTags(GrpcArticleService.Empty request, ServerCallContext context) // <-- THÊM PHƯƠNG THỨC NÀY
        {
            var list = new TagList();
            try
            {
                var tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
                list.Tags.AddRange(tags.Select(t => new TagResponse
                {
                    Id = t.TagId,
                    Name = t.Name
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả tags.");
                throw new RpcException(new Status(StatusCode.Internal, $"Lỗi khi lấy tất cả tags: {ex.Message}"));
            }
            return list;
        }
        public override async Task<CategoryList> GetAllCategories(GrpcArticleService.Empty request, ServerCallContext context)
        {
            var list = new CategoryList();
            try
            {
                var categories = await _context.Categories.ToListAsync(); // <-- Đảm bảo bạn có DbSet<Category>
                list.Categories.AddRange(categories.Select(c => new CategoryResponse
                {
                    Id = c.CategoryId,
                    Name = c.Name // <-- Đảm bảo tên thuộc tính CategoryName của bạn
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả danh mục.");
                throw new RpcException(new Status(StatusCode.Internal, $"Lỗi khi lấy tất cả danh mục: {ex.Message}"));
            }
            return list;
        }
        // --- END NEW ---

        // Trong GrpcMicroservice/Services/ArticleService.cs

        public override async Task<ArticleResponse> CreateArticle(CreateArticleRequest request, ServerCallContext context)
        {
            // Kiểm tra CategoryId
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Danh mục với ID '{request.CategoryId}' không tồn tại."));
            }

            var article = new Article
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = request.AuthorId,
                CreatedAt = DateTime.UtcNow,
                ImageUrl = request.ImageUrl,
                CategoryId = request.CategoryId,
                Status = "Draft", // Mặc định là Draft
                Tags = new List<Tag>() // Khởi tạo Tags collection rỗng
            };

            // Xử lý Tags cho Article mới - CHỈ TÌM KIẾM, KHÔNG TẠO MỚI
            foreach (var tagName in request.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var normalizedTagName = tagName.Trim();

                // Tìm tag hiện có trong database (không phân biệt chữ hoa/thường)
                var tag = await _context.Tags
                                        .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedTagName.ToLower());

                // Nếu tag không tồn tại trong DB, ném lỗi
                if (tag == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Tag '{tagName}' không tồn tại trong hệ thống."));
                }

                // Thêm tag vào collection Tags của bài viết
                article.Tags.Add(tag);
            }

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Article created: {Id}", article.ArticleId);

            // Chuẩn bị response
            return new ArticleResponse
            {
                Id = article.ArticleId.ToString(),
                Title = article.Title,
                Content = article.Content,
                AuthorId = article.AuthorId,
                CreatedAt = article.CreatedAt.HasValue ? article.CreatedAt.Value.ToString("o") : string.Empty,
                ImageUrl = article.ImageUrl ?? string.Empty,
                CategoryName = category.Name,
                Tags = { article.Tags.Select(t => t.Name).ToList() }
            };
        }

        public override async Task<ArticleResponse> UpdateArticle(UpdateArticleRequest request, ServerCallContext context)
        {
            if (!int.TryParse(request.Id, out int articleId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID bài báo không hợp lệ."));
            }

            var article = await _context.Articles
                                        .Include(a => a.Category)
                                        .Include(a => a.Tags)
                                        .FirstOrDefaultAsync(a => a.ArticleId == articleId);

            if (article == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Bài báo với ID '{request.Id}' không tìm thấy."));
            }

            // Kiểm tra CategoryId mới
            var newCategory = await _context.Categories.FindAsync(request.CategoryId);
            if (newCategory == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Danh mục với ID '{request.CategoryId}' không tồn tại."));
            }

            article.Title = request.Title;
            article.Content = request.Content;
            article.ImageUrl = request.ImageUrl;
            article.CategoryId = request.CategoryId;
            article.UpdatedAt = DateTime.UtcNow;

            // Cập nhật Tags: Xóa tags cũ và thêm tags mới - CHỈ TÌM KIẾM, KHÔNG TẠO MỚI
            article.Tags.Clear(); // Xóa tất cả các liên kết tag hiện có

            foreach (var tagName in request.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var normalizedTagName = tagName.Trim();

                // Tìm tag hiện có trong database (không phân biệt chữ hoa/thường)
                var existingTag = await _context.Tags
                                                .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedTagName.ToLower());

                // Nếu tag không tồn tại trong DB, ném lỗi
                if (existingTag == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Tag '{tagName}' không tồn tại trong hệ thống."));
                }

                // Thêm tag vào collection Tags của bài viết
                article.Tags.Add(existingTag);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Article updated: {Id}", article.ArticleId);

            // Chuẩn bị response
            return new ArticleResponse
            {
                Id = article.ArticleId.ToString(),
                Title = article.Title,
                Content = article.Content,
                AuthorId = article.AuthorId,
                CreatedAt = article.CreatedAt.HasValue ? article.CreatedAt.Value.ToString("o") : string.Empty,
                ImageUrl = article.ImageUrl ?? string.Empty,
                CategoryName = newCategory.Name,
                Tags = { article.Tags.Select(t => t.Name).ToList() }
            };
        }

        // GrpcMicroservice/Services/ArticleService.cs

        public override async Task<GrpcArticleService.DeleteArticleResult> DeleteArticle(DeleteArticleRequest request, ServerCallContext context)
        {
            // Kiểm tra xem ID có hợp lệ không
            if (!int.TryParse(request.Id, out int articleIdToDelete))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID bài báo không hợp lệ để xóa."));
            }

            try
            {
                // Sử dụng FindAsync để tìm bài báo bằng ID đã chuyển đổi sang int
                // Dòng 191 (hoặc tương đương)
                var articleToDelete = await _context.Articles.FindAsync(articleIdToDelete);

                if (articleToDelete == null)
                {
                    // Ném lỗi NotFound nếu không tìm thấy bài báo
                    throw new RpcException(new Status(StatusCode.NotFound, $"Bài báo với ID '{request.Id}' không tìm thấy để xóa."));
                }

                // Tùy chọn 1: Xóa mềm (soft delete) - được khuyến nghị hơn xóa cứng
                articleToDelete.IsDeleted = true;
                articleToDelete.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian xóa

                // Tùy chọn 2: Xóa cứng (hard delete) - Nếu bạn muốn xóa vĩnh viễn khỏi DB
                // _context.Articles.Remove(articleToDelete);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Article {ArticleId} marked as deleted by soft delete.", articleIdToDelete);

                return new GrpcArticleService.DeleteArticleResult { Success= true  }; // Giả định Empty message của bạn có thuộc tính Success
            }
            catch (RpcException)
            {
                // Re-throw RpcException đã tạo ở trên
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài báo với ID {ArticleId}", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, $"Lỗi server khi xóa bài báo: {ex.Message}"));
            }
        }
        // MỚI: Lấy các bài báo cần phê duyệt (Status = "Draft", IsDeleted = false)
        public override async Task<ArticleList> GetPendingArticles(GrpcArticleService.Empty request, ServerCallContext context)
        {
            try
            {
                var articles = await _context.Articles
                                             .Include(a => a.Category)
                                             .Include(a => a.Tags)
                                             .Include(a => a.Author) // Cần include Author để lấy thông tin tác giả
                                             .Where(a => a.Status == "Draft" && a.IsDeleted != true) // Lọc Status = "Draft"
                                             .OrderByDescending(a => a.CreatedAt)
                                             .ToListAsync();

                var response = new ArticleList();
                foreach (var article in articles)
                {
                    response.Articles.Add(new ArticleResponse
                    {
                        Id = article.ArticleId.ToString(),
                        Title = article.Title,
                        Content = article.Content,
                        ImageUrl = article.ImageUrl ?? string.Empty,
                        AuthorId = article.AuthorId,
                        AuthorName = article.Author?.UserName ?? "Unknown",
                        CategoryName = article.Category?.Name ?? "N/A",
                        CreatedAt = article.CreatedAt.HasValue ? article.CreatedAt.Value.ToString("o") : string.Empty,
                        PublishedDate = article.PublishedDate.HasValue ? article.PublishedDate.Value.ToString("o") : string.Empty,
                        Status = article.Status, // Trả về trạng thái Status
                        IsDeleted = article.IsDeleted ?? false, // Trả về trạng thái IsDeleted
                        Tags = { article.Tags.Select(t => t.Name).ToList() }
                    });
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi server khi lấy các bài báo cần phê duyệt.");
                throw new RpcException(new Status(StatusCode.Internal, "Lỗi server khi lấy bài báo cần phê duyệt."));
            }
        }

        // MỚI: Xuất bản bài báo (Admin xác nhận từ Draft thành Published)
        public override async Task<PublishArticleResponse> PublishArticle(PublishArticleRequest request, ServerCallContext context)
        {
            if (!int.TryParse(request.ArticleId, out int articleId))
            {
                return new PublishArticleResponse { Success = false, Message = "ID bài báo không hợp lệ." };
            }

            var article = await _context.Articles.FindAsync(articleId);

            if (article == null)
            {
                return new PublishArticleResponse { Success = false, Message = $"Bài báo với ID '{request.ArticleId}' không tìm thấy." };
            }

            if (article.Status == "Published")
            {
                return new PublishArticleResponse { Success = false, Message = "Bài báo đã được xuất bản trước đó." };
            }

            article.Status = "Published";
            article.PublishedDate = DateTime.UtcNow; // Đặt ngày xuất bản
            article.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Bài báo {ArticleId} đã được Admin xuất bản (Status = Published).", articleId);
                return new PublishArticleResponse { Success = true, Message = "Bài báo đã được xuất bản thành công." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất bản bài báo {ArticleId}.", articleId);
                return new PublishArticleResponse { Success = false, Message = $"Lỗi server: {ex.Message}" };
            }
        }

        // MỚI: Gỡ bài báo (Admin cho IsDeleted = true)
        public override async Task<RemoveArticleResponse> RemoveArticle(RemoveArticleRequest request, ServerCallContext context)
        {
            if (!int.TryParse(request.ArticleId, out int articleId))
            {
                return new RemoveArticleResponse { Success = false, Message = "ID bài báo không hợp lệ." };
            }

            var article = await _context.Articles.FindAsync(articleId);

            if (article == null)
            {
                return new RemoveArticleResponse { Success = false, Message = $"Bài báo với ID '{request.ArticleId}' không tìm thấy." };
            }

            if (article.IsDeleted == true)
            {
                return new RemoveArticleResponse { Success = false, Message = "Bài báo đã bị gỡ trước đó." };
            }

            article.IsDeleted = true; // <-- Đặt IsDeleted = true để gỡ bài
            article.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Bài báo {ArticleId} đã được Admin gỡ (IsDeleted = true).", articleId);
                return new RemoveArticleResponse { Success = true, Message = "Bài báo đã được gỡ thành công." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gỡ bài báo {ArticleId}.", articleId);
                return new RemoveArticleResponse { Success = false, Message = $"Lỗi server: {ex.Message}" };
            }
        }
        public override async Task<RestoreArticleResponse> RestoreArticle(RestoreArticleRequest request, ServerCallContext context)
        {
            if (!int.TryParse(request.ArticleId, out int articleId))
            {
                return new RestoreArticleResponse { Success = false, Message = "ID bài báo không hợp lệ." };
            }

            var article = await _context.Articles.FindAsync(articleId);

            if (article == null)
            {
                return new RestoreArticleResponse { Success = false, Message = $"Bài báo với ID '{request.ArticleId}' không tìm thấy." };
            }

            if (article.IsDeleted == false) // Kiểm tra nếu bài báo đã không bị gỡ
            {
                return new RestoreArticleResponse { Success = false, Message = "Bài báo chưa bị gỡ." };
            }

            article.IsDeleted = false; // Đặt IsDeleted thành false để khôi phục
            article.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian chỉnh sửa

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Bài báo {ArticleId} đã được Admin khôi phục (IsDeleted = false).", articleId);
                return new RestoreArticleResponse { Success = true, Message = "Bài báo đã được khôi phục thành công." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khôi phục bài báo {ArticleId}.", articleId);
                return new RestoreArticleResponse { Success = false, Message = $"Lỗi server: {ex.Message}" };
            }
        }
        // MỚI/XÁC NHẬN: Triển khai phương thức GetAllArticlesForAdmin
        public override async Task<ArticleList> GetAllArticlesForAdmin(GrpcArticleService.Empty request, ServerCallContext context)
        {
            try
            {
                // Lấy TẤT CẢ các bài báo, không lọc theo Status hay IsDeleted
                var articles = await _context.Articles
                                             .Include(a => a.Category)
                                             .Include(a => a.Tags)
                                             .Include(a => a.Author) // Đảm bảo include Author để lấy AuthorName
                                             .OrderByDescending(a => a.CreatedAt) // Sắp xếp theo ngày tạo hoặc PublishedDate
                                             .ToListAsync();

                var response = new ArticleList(); // Khởi tạo ArticlesResponse
                foreach (var article in articles)
                {
                    response.Articles.Add(new ArticleResponse // Thêm từng ArticleResponse vào danh sách
                    {
                        Id = article.ArticleId.ToString(),
                        Title = article.Title,
                        Content = article.Content,
                        ImageUrl = article.ImageUrl ?? string.Empty,
                        AuthorId = article.AuthorId,
                        AuthorName = article.Author?.UserName ?? "Unknown", // Đảm bảo lấy được tên tác giả
                        CategoryName = article.Category?.Name ?? "N/A",
                        CreatedAt = article.CreatedAt.HasValue ? article.CreatedAt.Value.ToString("o") : string.Empty,
                        PublishedDate = article.PublishedDate.HasValue ? article.PublishedDate.Value.ToString("o") : string.Empty,
                        Status = article.Status,
                        IsDeleted = article.IsDeleted ?? false,
                        Tags = { article.Tags.Select(t => t.Name).ToList() }
                    });
                }
                return response; // Trả về ArticlesResponse chứa danh sách
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi server khi lấy tất cả bài báo cho Admin.");
                throw new RpcException(new Status(StatusCode.Internal, "Lỗi server khi lấy tất cả bài báo cho Admin."));
            }
        }
        // THÊM PHƯƠNG THỨC GetAllArticles
        public override async Task<ArticleList> GetAllArticles(GrpcArticleService.Empty request, ServerCallContext context)
        {
            var list = new ArticleList();
            try
            {
                // Include Category để lấy tên, Include Tags để lấy danh sách tags
                var articles = await _context.Articles
                                            .Include(a => a.Category)
                                            .Include(a => a.Tags)
                                            .Where(c=>c.IsDeleted==false ) // <-- LỌC CÁC BÀI VIẾT CHƯA BỊ XÓA
                                            .ToListAsync();
                list.Articles.AddRange(articles.Select(a => new ArticleResponse
                {
                    Id = a.ArticleId.ToString(),
                    Title = a.Title,
                    Content = a.Content,
                    AuthorId = a.AuthorId,
                    CreatedAt = a.CreatedAt.HasValue ? a.CreatedAt.Value.ToString("o") : string.Empty,
                    ImageUrl = a.ImageUrl ?? string.Empty,
                    CategoryName = a.Category?.Name ?? "N/A", // <-- TRẢ VỀ TÊN DANH MỤC
                    Tags = { a.Tags.Select(t => t.Name).ToList() } // <-- TRẢ VỀ DANH SÁCH TAG
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả bài báo.");
                throw new RpcException(new Status(StatusCode.Internal, $"Lỗi khi lấy tất cả bài báo: {ex.Message}"));
            }
            return list;
        }

        public override async Task<ArticleResponse> GetArticleById(DeleteArticleRequest request, ServerCallContext context)
        {
            if (!int.TryParse(request.Id, out int articleId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID bài báo không hợp lệ."));
            }

            var article = await _context.Articles
                                        .Include(a => a.Category)
                                        .Include(a => a.Tags)
                                        .FirstOrDefaultAsync(a => a.ArticleId == articleId);
            if (article == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Bài báo với ID '{request.Id}' không tìm thấy."));
            }

            return new ArticleResponse
            {
                Id = article.ArticleId.ToString(),
                Title = article.Title,
                Content = article.Content,
                AuthorId = article.AuthorId,
                CreatedAt = article.CreatedAt.HasValue ? article.CreatedAt.Value.ToString("o") : string.Empty,
                ImageUrl = article.ImageUrl ?? string.Empty,
                CategoryName = article.Category?.Name ?? "N/A", // <-- TRẢ VỀ TÊN DANH MỤC
                Tags = { article.Tags.Select(t => t.Name).ToList() } // <-- TRẢ VỀ DANH SÁCH TAG
            };
        }
    }

}
