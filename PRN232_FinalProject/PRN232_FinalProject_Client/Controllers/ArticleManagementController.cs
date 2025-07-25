using Grpc.Core;
using GrpcArticleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN232_FinalProject.Services.Implement;
using PRN232_FinalProject.Services.Interfaces;
using PRN232_FinalProject_Client.Models;
using System.Security.Claims;

namespace PRN232_FinalProject_Client.Controllers
{
    [Authorize(Roles = "Lecturer, Staff")]
    public class ArticleManagementController : Controller
    {
        private readonly GrpcArticleService.ArticleService.ArticleServiceClient _articleClient;
        private readonly ILogger<ArticleManagementController> _logger;
        private readonly ICloudinaryService _cloudinaryService;
        public ArticleManagementController(
            GrpcArticleService.ArticleService.ArticleServiceClient articleClient,
            ILogger<ArticleManagementController> logger, ICloudinaryService cloudinaryService)
        {
            _articleClient = articleClient;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }
        private async Task LoadCategoriesAndTagsIntoViewModel(dynamic model)
        {
            try
            {
                // Tải Categories
                var categoriesResponse = await _articleClient.GetAllCategoriesAsync(new Empty());
                model.Categories = categoriesResponse.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = (model.CategoryId == c.Id)
                }).ToList();

                // Tải Tags
                var tagsResponse = await _articleClient.GetAllTagsAsync(new Empty());
                var allTags = tagsResponse.Tags.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();

                // Tạo MultiSelectList cho AvailableTags
                model.AvailableTags = new MultiSelectList(allTags, "Value", "Text", model.SelectedTagIds);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi tải danh mục hoặc tags: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                TempData["ErrorMessage"] = $"Không thể tải danh mục hoặc tags: {ex.Status.Detail}";
                model.Categories = new List<SelectListItem>();
                model.AvailableTags = new MultiSelectList(new List<SelectListItem>()); // Trả về rỗng nếu có lỗi
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi tải danh mục hoặc tags.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tải danh mục hoặc tags: {ex.Message}";
                model.Categories = new List<SelectListItem>();
                model.AvailableTags = new MultiSelectList(new List<SelectListItem>());
            }
        }
        // Helper method to load categories
        private async Task LoadCategoriesIntoViewModel(dynamic model)
        {
            try
            {
                var categoriesResponse = await _articleClient.GetAllCategoriesAsync(new Empty());
                model.Categories = categoriesResponse.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == model.CategoryId // Set selected if it's for Edit
                }).ToList();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi tải danh mục: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                TempData["ErrorMessage"] = $"Không thể tải danh mục: {ex.Status.Detail}";
                model.Categories = new List<SelectListItem>(); // Trả về danh sách rỗng nếu có lỗi
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy AuthorId của người dùng hiện tại
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var model = new ArticleListViewModel();
            model.Articles = new List<ArticleResponse>(); // Khởi tạo danh sách rỗng để tránh NullReferenceException

            try
            {
                // 1. Gọi gRPC để lấy TẤT CẢ bài báo
                // (Đây là cách bạn đang làm, lấy tất cả rồi lọc sau)
                var allArticlesResponse = await _articleClient.GetAllArticlesAsync(new Empty());

                // 2. Lọc bài báo DỰA TRÊN loggedInUserId ở phía client
                if (loggedInUserId != null)
                {
                    // So sánh AuthorId (từ gRPC response) với loggedInUserId
                    // Cẩn thận với so sánh chuỗi (case-sensitive/insensitive)
                    model.Articles = allArticlesResponse.Articles
                                                        .Where(c => c.AuthorId.Equals(loggedInUserId, StringComparison.OrdinalIgnoreCase)) // Sử dụng StringComparison để so sánh không phân biệt chữ hoa/thường
                                                        .ToList(); // Chuyển kết quả lọc thành List
                }
                else
                {
                    // Nếu người dùng chưa đăng nhập, trả về danh sách rỗng
                    // (Hoặc có thể redirect đến trang đăng nhập/thông báo, tùy logic mong muốn)
                    model.Articles = new List<ArticleResponse>();
                    TempData["InfoMessage"] = "Vui lòng đăng nhập để xem các bài viết của bạn.";
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi lấy danh sách bài báo: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                TempData["ErrorMessage"] = $"Không thể tải danh sách bài báo: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi lấy danh sách bài báo.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateArticleViewModel();
            await LoadCategoriesAndTagsIntoViewModel(model); // Gọi hàm mới
            return View(model);
        }

        // POST: /ArticleManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateArticleViewModel model)
        {
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                ModelState.AddModelError("", "Không thể xác định ID tác giả. Vui lòng đăng nhập lại.");
                await LoadCategoriesAndTagsIntoViewModel(model);
                return View(model);
            }
            model.AuthorId = loggedInUserId;

            // Xử lý upload ảnh
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                try
                {
                    var imageUrl = await _cloudinaryService.UploadImageAsync(model.ImageFile);
                    model.ImageUrl = imageUrl;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, "Lỗi cấu hình Cloudinary khi tải ảnh lên: {Message}", ex.Message);
                    ModelState.AddModelError("ImageFile", $"Lỗi cấu hình Cloudinary: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tải ảnh lên Cloudinary.");
                    ModelState.AddModelError("ImageFile", $"Có lỗi xảy ra khi tải ảnh lên: {ex.Message}");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndTagsIntoViewModel(model); // Gọi hàm mới
                return View(model);
            }

            // --- GỌI GRPC ĐỂ TẠO BÀI VIẾT VỚI URL ĐÃ CÓ VÀ CÁC TAGS ĐƯỢC CHỌN ---
            try
            {
               
                var selectedTagNames = new List<string>();
                if (model.SelectedTagIds != null && model.SelectedTagIds.Any())
                {
                    var allTagsResponse = await _articleClient.GetAllTagsAsync(new Empty());
                    selectedTagNames = allTagsResponse.Tags
                                            .Where(t => model.SelectedTagIds.Contains(t.Id))
                                            .Select(t => t.Name)
                                            .ToList();
                }

                var request = new CreateArticleRequest
                {
                    Title = model.Title,
                    Content = model.Content,
                    AuthorId = loggedInUserId,
                    ImageUrl = model.ImageUrl ?? string.Empty,
                    CategoryId = model.CategoryId,
                    Tags = { selectedTagNames } // Gán DANH SÁCH TÊN TAGS
                };  

                var response = await _articleClient.CreateArticleAsync(request);
                TempData["SuccessMessage"] = $"Bài báo '{response.Title}' (ID: {response.Id}) đã được tạo thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi tạo bài báo: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                ModelState.AddModelError("", $"Lỗi khi tạo bài báo: {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi tạo bài báo.");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }

            await LoadCategoriesAndTagsIntoViewModel(model); // Gọi hàm mới
            return View(model);
        }

        // GET: /ArticleManagement/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bài báo không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditArticleViewModel();
            try
            {
                var articleResponse = await _articleClient.GetArticleByIdAsync(new DeleteArticleRequest { Id = id });

                model.Id = articleResponse.Id;
                model.Title = articleResponse.Title;
                model.Content = articleResponse.Content;
                model.AuthorId = articleResponse.AuthorId;
                model.CreatedAt = articleResponse.CreatedAt;
                model.ImageUrl = articleResponse.ImageUrl;
                model.CategoryId = articleResponse.CategoryId;

                
                var allTagsResponse = await _articleClient.GetAllTagsAsync(new Empty());
                var allTagsMap = allTagsResponse.Tags.ToDictionary(t => t.Name, t => t.Id);

                model.SelectedTagIds = articleResponse.Tags
                                            .Where(tagName => allTagsMap.ContainsKey(tagName))
                                            .Select(tagName => allTagsMap[tagName])
                                            .ToList();

                await LoadCategoriesAndTagsIntoViewModel(model); // Gọi hàm mới
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    _logger.LogWarning("Bài báo với ID '{ArticleId}' không tìm thấy để chỉnh sửa. Lỗi: {Detail}", id, ex.Status.Detail);
                    TempData["ErrorMessage"] = $"Không tìm thấy bài báo với ID: {id}";
                }
                else
                {
                    _logger.LogError(ex, "Lỗi gRPC khi lấy thông tin bài báo: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                    TempData["ErrorMessage"] = $"Lỗi khi tải thông tin bài báo: {ex.Status.Detail}";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi lấy thông tin bài báo.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /ArticleManagement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditArticleViewModel model)
        {
            // Xử lý upload ảnh
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                try
                {
                    var imageUrl = await _cloudinaryService.UploadImageAsync(model.ImageFile);
                    model.ImageUrl = imageUrl;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, "Lỗi cấu hình Cloudinary khi tải ảnh lên: {Message}", ex.Message);
                    ModelState.AddModelError("ImageFile", $"Lỗi cấu hình Cloudinary: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tải ảnh lên Cloudinary.");
                    ModelState.AddModelError("ImageFile", $"Có lỗi xảy ra khi tải ảnh lên: {ex.Message}");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndTagsIntoViewModel(model); // Gọi hàm mới
                return View(model);
            }

            try
            {
                // Lấy tên tags từ ID đã chọn
                var selectedTagNames = new List<string>();
                if (model.SelectedTagIds != null && model.SelectedTagIds.Any())
                {
                    var allTagsResponse = await _articleClient.GetAllTagsAsync(new Empty());
                    selectedTagNames = allTagsResponse.Tags
                                            .Where(t => model.SelectedTagIds.Contains(t.Id))
                                            .Select(t => t.Name)
                                            .ToList();
                }

                var request = new UpdateArticleRequest
                {
                    Id = model.Id,
                    Title = model.Title,
                    Content = model.Content,
                    ImageUrl = model.ImageUrl ?? string.Empty,
                    CategoryId = model.CategoryId,
                    Tags = { selectedTagNames } // Gán DANH SÁCH TÊN TAGS
                };

                var response = await _articleClient.UpdateArticleAsync(request);
                TempData["SuccessMessage"] = $"Bài báo '{response.Title}' (ID: {response.Id}) đã được cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi cập nhật bài báo: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                ModelState.AddModelError("", $"Lỗi khi cập nhật bài báo: {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi cập nhật bài báo.");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }
            await LoadCategoriesAndTagsIntoViewModel(model); // Gọi hàm mới
            return View(model);
        }
        [HttpPost]
        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bài báo không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy AuthorId của người dùng hiện tại
            var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng nhập hoặc không thể xác định tài khoản của bạn.";
                return RedirectToAction(nameof(Index)); // Hoặc RedirectToAction("Login", "Auth2");
            }

            try
            {
                // 1. Lấy thông tin bài báo từ gRPC server để kiểm tra quyền sở hữu
                var getArticleRequest = new GrpcArticleService.DeleteArticleRequest { Id = id };
                var articleResponse = await _articleClient.GetArticleByIdAsync(getArticleRequest);

                // Kiểm tra nếu bài báo không tồn tại
                if (articleResponse == null || string.IsNullOrEmpty(articleResponse.Id))
                {
                    TempData["ErrorMessage"] = $"Bài báo với ID '{id}' không tìm thấy.";
                    return RedirectToAction(nameof(Index));
                }

                // 2. So sánh AuthorId của bài báo với AuthorId của người dùng đang đăng nhập
                // So sánh không phân biệt chữ hoa/thường vì AuthorId có thể là GUID.
                if (!articleResponse.AuthorId.Equals(loggedInUserId, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xóa bài báo này vì bạn không phải là tác giả.";
                    return RedirectToAction(nameof(Index));
                }

                // 3. Nếu quyền sở hữu hợp lệ, tiến hành gửi yêu cầu xóa tới gRPC server
                var deleteRequest = new GrpcArticleService.DeleteArticleRequest { Id = id };
                var deleteResponse = await _articleClient.DeleteArticleAsync(deleteRequest);

                if (deleteResponse.Success)
                {
                    TempData["SuccessMessage"] = $"Bài báo với ID '{id}' đã được xóa thành công.";
                }
                else
                {
                    // Xử lý trường hợp server báo xóa thất bại (ví dụ: bài báo đã bị xóa trước đó)
                    TempData["ErrorMessage"] = $"Xóa bài báo với ID '{id}' thất bại.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                // Xử lý lỗi Not Found riêng nếu server ném lỗi cụ thể
                _logger.LogWarning(ex, "Bài báo với ID '{Id}' không tìm thấy khi cố gắng xóa.", id);
                TempData["ErrorMessage"] = $"Bài báo với ID '{id}' không tồn tại hoặc đã bị xóa.";
                return RedirectToAction(nameof(Index));
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi xóa bài báo: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                TempData["ErrorMessage"] = $"Lỗi khi xóa bài báo: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi xóa bài báo.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
