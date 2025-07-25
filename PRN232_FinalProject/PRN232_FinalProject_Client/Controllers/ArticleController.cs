using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject_Client.DTO;
using PRN232_FinalProject_Client.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN232_FinalProject_Client.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ArticleService _articleService;

        public ArticleController(ArticleService articleService)
        {
            _articleService = articleService;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetArticlesAsync();
            return View(articles);
        }
            
        // GET: /Article/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);

            if (article == null)
            {
                return NotFound();
            }
            ViewBag.LikeCount = article.LikesCount;

            // ✅ Kiểm tra người dùng hiện tại đã Like chưa
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var token = HttpContext.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(userId))
            {   
                ViewBag.HasLiked = await _articleService.IsLikedByUserAsync(id, userId,token);
            }
            else
            {
                ViewBag.HasLiked = false; // Chưa đăng nhập
            }
            // Lấy các bài viết liên quan (ví dụ: cùng Category)
            var allArticles = await _articleService.GetArticlesAsync(); // Hoặc gọi API filter nếu có
            var relatedArticles = allArticles
                                .Where(a => a.CategoryId == article.CategoryId && a.ArticleID != id)
                                .Take(5) // Lấy 5 bài viết liên quan
                                .ToList();

            ViewBag.RelatedArticles = relatedArticles;

            // Lấy bình luận từ API và truyền vào ViewBag
            // Đảm bảo CommentTreeDto đã được định nghĩa trong PRN232_FinalProject_Client.DTO
            try
            {
                var comments = await _articleService.GetCommentsTreeByArticleAsync(id);
                ViewBag.Comments = comments;
            }
            catch (HttpRequestException ex)
            {
                // Xử lý lỗi khi gọi API bình luận, ví dụ: log lỗi hoặc trả về danh sách rỗng
                ViewBag.Comments = new List<CommentTreeDto>();
                ViewBag.CommentError = $"Failed to load comments: {ex.Message}";
                Console.WriteLine($"Error loading comments: {ex.Message}");
            }


            // Lấy current user ID để truyền xuống view cho các nút edit/delete
            ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.CurrentUserName = User.Identity?.Name;

            return View(article);
        }
        [HttpPost]
        public async Task<IActionResult> ToggleLike(int articleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Nếu chưa đăng nhập
            }

            try
            {
                var token = HttpContext.Session.GetString("JWT");
                var isLiked = await _articleService.IsLikedByUserAsync(articleId, userId, token);
                bool success;
                if (isLiked)
                {
                    success = await _articleService.UnlikeArticleAsync(articleId, userId, token);
                }
                else
                {
                    success = await _articleService.LikeArticleAsync(articleId, userId, token);
                }

                // ✅ Redirect về trang chi tiết bài viết
                return RedirectToAction("Details", new { id = articleId });
            }
            catch (Exception)
            {
                // Có lỗi thì vẫn redirect về Details và có thể hiển thị thông báo lỗi ở View nếu cần
                TempData["LikeError"] = "Đã xảy ra lỗi khi xử lý thích bài viết.";
                return RedirectToAction("Details", new { id = articleId });
            }
        }



        // Action để xử lý việc thêm bình luận mới (sử dụng form POST truyền thống)
        [HttpPost]
        [ValidateAntiForgeryToken] // Nên thêm để chống CSRF
        public async Task<IActionResult> AddComment(int id, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Comment content cannot be empty.";
                return RedirectToAction("Details", new { id = id });
            }

            var token = HttpContext.Session.GetString("JWT"); // Lấy token từ session
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để bình luận.";
                return RedirectToAction("Details", new { id = id });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy User ID
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["ErrorMessage"] = "Không thể xác định người dùng.";
                return RedirectToAction("Details", new { id = id });
            }

            var commentDto = new CommentDto
            {
                Content = content,
                ArticleId = id,
                AuthorId = currentUserId,
                ParentCommentId = parentCommentId // Có thể là null hoặc ID của bình luận cha
            };

            try
            {
                var success = await _articleService.PostCommentAsync(commentDto, token);

                
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi gửi bình luận: {ex.Message}";
                Console.WriteLine($"Error posting comment: {ex.Message}");
            }


            return RedirectToAction("Details", new { id = id });
        }

        // Action để xử lý việc cập nhật bình luận (sử dụng form POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(int id, int commentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction("Details", new { id = id });
            }

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để sửa bình luận.";
                return RedirectToAction("Details", new { id = id });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Kiểm tra xem người dùng có quyền sửa bình luận này không (cần lấy comment cũ để so sánh AuthorId)
            // Để đơn giản, tôi sẽ bỏ qua bước này ở đây, nhưng trong thực tế bạn NÊN kiểm tra quyền ở BE
            // (server-side của Comment API cũng đã kiểm tra rồi).

            var commentUpdateDto = new CommentUpdateDto
            {
                CommentId = commentId,
                Content = content
            };

            try
            {
                var success = await _articleService.UpdateCommentAsync(commentId, commentUpdateDto, token);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật bình luận. Vui lòng thử lại.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật bình luận: {ex.Message}";
                Console.WriteLine($"Error updating comment: {ex.Message}");
            }

            return RedirectToAction("Details", new { id = id });
        }

        // Action để xử lý việc xóa bình luận (xóa mềm, sử dụng form POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, int commentId)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xóa bình luận.";
                return RedirectToAction("Details", new { id = id });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Kiểm tra quyền ở đây (tương tự như UpdateComment)

            try
            {
                var success = await _articleService.DeleteCommentAsync(commentId, token);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Không thể xóa bình luận. Vui lòng thử lại.";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa bình luận: {ex.Message}";
                Console.WriteLine($"Error deleting comment: {ex.Message}");
            }

            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
            public IActionResult Create()
            {
                var token = HttpContext.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Create(ArticleCreateDto dto)
            {
                var token = HttpContext.Session.GetString("JWT");
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");
                if (!ModelState.IsValid) return View(dto);
                var result = await _articleService.CreateArticleAsync(dto, token);
                if (result)
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Tạo bài báo thất bại");
                return View(dto);
            }
            }
            }
