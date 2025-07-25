// PRN232_FinalProject_Client/Controllers/AdminController.cs

using Grpc.Core; // Để bắt RpcException
using GrpcArticleService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Grpc; // Namespace cho AccountService (đã định nghĩa trong account.proto)
using PRN232_FinalProject_Client.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PRN232_FinalProject_Client.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ArticleService.ArticleServiceClient _articleClient;
        private readonly AccountService.AccountServiceClient _accountClient;
        private readonly ILogger<AdminController> _logger; // Nên có Logger để ghi log lỗi

        public AdminController(AccountService.AccountServiceClient accountClient, ILogger<AdminController> logger, ArticleService.ArticleServiceClient articleClient)
        {
            _accountClient = accountClient;
            _logger = logger;
            _articleClient = articleClient;
        }
        public async Task<IActionResult> PendingArticles()
        {
            var model = new Models.ArticleListViewModel(); // Sử dụng ViewModel đã có
            try
            {
                // Gọi RPC để lấy danh sách các bài báo đang chờ duyệt
                var response = await _articleClient.GetPendingArticlesAsync(new GrpcArticleService.Empty());
                model.Articles = response.Articles.ToList();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi lấy danh sách bài báo chờ duyệt: {Status}", ex.Status.Detail);
                TempData["ErrorMessage"] = $"Không thể tải bài báo chờ duyệt: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi tải bài báo chờ duyệt.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return View(model);
        }

        // POST: AdminArticle/PublishArticle
        // Hành động để chuyển trạng thái bài báo từ "Draft" sang "Published"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishArticle(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bài báo không hợp lệ.";
                return RedirectToAction(nameof(PendingArticles));
            }

            try
            {
                var request = new PublishArticleRequest { ArticleId = id };
                var response = await _articleClient.PublishArticleAsync(request);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi xuất bản bài báo: {Status}", ex.Status.Detail);
                TempData["ErrorMessage"] = $"Lỗi khi xuất bản bài báo: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi xuất bản bài báo.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return RedirectToAction(nameof(PendingArticles));
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveArticle(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bài báo không hợp lệ.";
                return RedirectToAction(nameof(AllArticles)); // Chuyển hướng về trang AllArticles sau khi gỡ
            }

            try
            {
                var request = new RemoveArticleRequest { ArticleId = id };
                var response = await _articleClient.RemoveArticleAsync(request);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi gỡ bài báo: {Status}", ex.Status.Detail);
                TempData["ErrorMessage"] = $"Lỗi khi gỡ bài báo: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi gỡ bài báo.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return RedirectToAction(nameof(AllArticles)); // Chuyển hướng về trang AllArticles
        }

        // POST: AdminArticle/RestoreArticle
        // Hành động để khôi phục bài báo bằng cách đặt IsDeleted = false
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreArticle(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bài báo không hợp lệ.";
                return RedirectToAction(nameof(AllArticles));
            }

            try
            {
                var request = new RestoreArticleRequest { ArticleId = id };
                var response = await _articleClient.RestoreArticleAsync(request);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi khôi phục bài báo: {Status}", ex.Status.Detail);
                TempData["ErrorMessage"] = $"Lỗi khi khôi phục bài báo: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi khôi phục bài báo.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return RedirectToAction(nameof(AllArticles));
        }

        // GET: AdminArticle/AllArticles
        // Hiển thị tất cả bài báo (bao gồm Draft, Published, IsDeleted) cho Admin
        public async Task<IActionResult> AllArticles()
        {
            var model = new Models.ArticleListViewModel();
            try
            {
                // Gọi RPC mới để lấy TẤT CẢ bài báo cho Admin (không lọc)
                var response = await _articleClient.GetAllArticlesForAdminAsync(new GrpcArticleService.Empty());
                model.Articles = response.Articles.ToList();
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi lấy tất cả bài báo cho Admin: {Status}", ex.Status.Detail);
                TempData["ErrorMessage"] = $"Không thể tải tất cả bài báo: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi tải tất cả bài báo cho Admin.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return View(model);
        }

        // Action tổng hợp để chuyển đổi trạng thái (tùy chọn, bạn có thể dùng các action riêng biệt ở trên)
        // Nó sẽ cố gắng thực hiện Publish, Remove hoặc Restore dựa trên trạng thái hiện tại.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleArticleState(string id, string currentStatus, bool isDeleted)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bài báo không hợp lệ.";
                return RedirectToAction(nameof(AllArticles));
            }

            try
            {
                if (isDeleted) // Nếu bài báo hiện đang bị gỡ (IsDeleted = true), thì khôi phục nó
                {
                    var restoreRequest = new RestoreArticleRequest { ArticleId = id };
                    var restoreResponse = await _articleClient.RestoreArticleAsync(restoreRequest);
                    if (restoreResponse.Success) TempData["SuccessMessage"] = restoreResponse.Message;
                    else TempData["ErrorMessage"] = restoreResponse.Message;
                }
                else // Nếu bài báo chưa bị gỡ (IsDeleted = false)
                {
                    if (currentStatus == "Draft") // Nếu bài báo đang là Draft, Admin có thể xuất bản
                    {
                        var publishRequest = new PublishArticleRequest { ArticleId = id };
                        var publishResponse = await _articleClient.PublishArticleAsync(publishRequest);
                        if (publishResponse.Success) TempData["SuccessMessage"] = publishResponse.Message;
                        else TempData["ErrorMessage"] = publishResponse.Message;
                    }
                    else if (currentStatus == "Published") // Nếu bài báo đang Published, Admin có thể gỡ (IsDeleted = true)
                    {
                        var removeRequest = new RemoveArticleRequest { ArticleId = id };
                        var removeResponse = await _articleClient.RemoveArticleAsync(removeRequest);
                        if (removeResponse.Success) TempData["SuccessMessage"] = removeResponse.Message;
                        else TempData["ErrorMessage"] = removeResponse.Message;
                    }
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi chuyển trạng thái bài báo: {Status}", ex.Status.Detail);
                TempData["ErrorMessage"] = $"Lỗi khi chuyển trạng thái bài báo: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi chuyển trạng thái bài báo.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return RedirectToAction(nameof(AllArticles));
        }

        // Action để hiển thị danh sách tất cả tài khoản
        public async Task<IActionResult> Index() // Đổi tên thành Index hoặc GetAccounts để dễ quản lý
        {
            try
            {
                // Gọi gRPC để lấy tất cả tài khoản
                var accountsList = await _accountClient.GetAllAccountsAsync(new MyProject.Grpc.Empty());

                // Truyền danh sách tài khoản sang View
                return View(accountsList.Accounts); // View này sẽ cần model là IEnumerable<AccountReply>
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi lấy danh sách tài khoản: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                ViewBag.ErrorMessage = $"Không thể tải danh sách tài khoản: {ex.Status.Detail}";
                return View(new List<AccountReply>()); // Trả về danh sách rỗng nếu có lỗi
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi lấy danh sách tài khoản.");
                ViewBag.ErrorMessage = $"Có lỗi xảy ra: {ex.Message}";
                return View(new List<AccountReply>());
            }
        }

        // Action để hiển thị form tạo tài khoản mới
        [HttpGet]
        public async Task<IActionResult> Create() // Thay đổi sang async để gọi gRPC
        {
            var model = new CreateAccountViewModel();
            try
            {
                // Gọi gRPC để lấy danh sách các vai trò
                var rolesResponse = await _accountClient.GetAllRolesAsync(new MyProject.Grpc.Empty());
                model.AvailableRoles = new List<string>(rolesResponse.Roles);

                // Thêm một lựa chọn mặc định hoặc rỗng nếu cần
                // model.AvailableRoles.Insert(0, "-- Chọn vai trò --");
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi lấy danh sách vai trò: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                TempData["ErrorMessage"] = $"Không thể tải danh sách vai trò: {ex.Status.Detail}";
                // Vẫn trả về View với danh sách rỗng, hoặc xử lý lỗi khác
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi lấy danh sách vai trò.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return View(model);
        }
       
        // Action để xử lý việc tạo tài khoản mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken] // Nên có để bảo vệ khỏi tấn công CSRF
        public async Task<IActionResult> Create(CreateAccountViewModel model) // Model từ form
        {
            //if (!ModelState.IsValid)
            //{

            //    return RedirectToAction(nameof(Create));
            //}
            try
            {
                // Ánh xạ từ ViewModel sang AccountRequest để gửi qua gRPC
                var accountRequest = new AccountRequest
                {
                    // Bạn có thể tạo UserId mới ở đây hoặc để trống nếu server tự gán
                    // Nếu UserId là bắt buộc ở server và không phải auto-generated, bạn cần sinh nó ở đây
                    UserId = Guid.NewGuid().ToString(),
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = model.Role,
                    Password = model.Password,
                    Username = model.Username
                };
                var reply = await _accountClient.CreateAccountAsync(accountRequest);
                if (reply != null && !string.IsNullOrEmpty(reply.Id))
                {
                    TempData["SuccessMessage"] = $"Tài khoản {reply.Id} đã được tạo thành công: ";
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
                }
                else
                {
                    ModelState.AddModelError("", "Tạo tài khoản thất bại: " );
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi tạo tài khoản: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                ModelState.AddModelError("", $"Lỗi khi tạo tài khoản: {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi tạo tài khoản.");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }
            return  RedirectToAction(nameof(Create));
        }

        // SỬA Action để hiển thị form chỉnh sửa tài khoản (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditAccountViewModel();
            try
            {
                var request = new AccountIdRequest { UserId = id };
                var accountGrpc = await _accountClient.GetAccountByIdAsync(request);

                // Ánh xạ AccountRequest sang EditAccountViewModel
                model.UserId = accountGrpc.UserId;
               
                model.Email = accountGrpc.Email;
                model.Role = accountGrpc.Role;
                // KHÔNG GÁN MẬT KHẨU VÀO NEWPASSWORD Ở ĐÂY!
                // model.NewPassword = accountGrpc.Password; // <-- LOẠI BỎ DÒNG NÀY

              
                await PopulateAvailableRolesForEdit(model);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
                {
                    _logger.LogWarning("Tài khoản với UserId '{UserId}' không tìm thấy để chỉnh sửa. Lỗi: {Detail}", id, ex.Status.Detail);
                    TempData["ErrorMessage"] = $"Không tìm thấy tài khoản với ID: {id}";
                }
                else
                {
                    _logger.LogError(ex, "Lỗi gRPC khi lấy thông tin tài khoản để chỉnh sửa: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                    TempData["ErrorMessage"] = $"Lỗi khi tải thông tin tài khoản: {ex.Status.Detail}";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi lấy thông tin tài khoản để chỉnh sửa.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // SỬA Action để xử lý việc cập nhật tài khoản (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            // RẤT QUAN TRỌNG: Bật lại kiểm tra ModelState.IsValid
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Cập nhật tài khoản thất bại: Dữ liệu không hợp lệ."); // Thông báo lỗi chung
                await PopulateAvailableRolesForEdit(model);
                return View(model);
            }

            try
            {
                // Lấy thông tin tài khoản hiện tại để giữ lại mật khẩu cũ, IsActive và ImageUrl nếu không thay đổi
                var currentAccountGrpc = await _accountClient.GetAccountByIdAsync(new AccountIdRequest { UserId = model.UserId });
                if (currentAccountGrpc == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy tài khoản để cập nhật.");
                    await PopulateAvailableRolesForEdit(model);
                    return View(model);
                }

                // Tạo AccountRequest để gửi đến gRPC service
                var accountRequest = new AccountRequest
                {
                    UserId = currentAccountGrpc.UserId,
                    Username = currentAccountGrpc.Username ?? "", // Đảm bảo không gửi null đến gRPC nếu Username là tùy chọn và null
                    FullName = currentAccountGrpc.FullName ?? "", // Đảm bảo không gửi null
                    Email = model.Email,
                    Role = model.Role ?? "User", // Cung cấp giá trị mặc định nếu Role là null
                                                 // LOGIC QUAN TRỌNG CHO MẬT KHẨU:
                                                 // Nếu model.NewPassword là null hoặc rỗng, giữ nguyên mật khẩu cũ từ currentAccountGrpc.Password
                                                 // Ngược lại, sử dụng mật khẩu mới đã nhập.
                    Password = string.IsNullOrEmpty(model.NewPassword) ? currentAccountGrpc.Password : model.NewPassword
                    // Đảm bảo không gửi null nếu ImageUrl là tùy chọn và null
                };

                var reply = await _accountClient.UpdateAccountAsync(accountRequest);

                // Sửa reply.Id thành reply.UserId
                if (reply != null && !string.IsNullOrEmpty(reply.Id))
                {
                    TempData["SuccessMessage"] = $"Tài khoản {reply.Id} đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Cập nhật tài khoản thất bại: Phản hồi không hợp lệ từ dịch vụ.");
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi cập nhật tài khoản: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                ModelState.AddModelError("", $"Lỗi khi cập nhật tài khoản: {ex.Status.Detail}");
                await PopulateAvailableRolesForEdit(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi cập nhật tài khoản.");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                await PopulateAvailableRolesForEdit(model);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiveStatus(string id) // Tên action chung
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var request = new AccountIdRequest { UserId = id };
                var updatedAccount = await _accountClient.ToggleAccountActiveAsync(request);

                string actionMessage = updatedAccount.IsActive ? "kích hoạt" : "vô hiệu hóa";
                TempData["SuccessMessage"] = $"Tài khoản {id} đã được {actionMessage} thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi chuyển đổi trạng thái tài khoản: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                TempData["ErrorMessage"] = $"Lỗi khi chuyển đổi trạng thái tài khoản: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi chuyển đổi trạng thái tài khoản.");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // Phương thức helper để tải danh sách vai trò từ gRPC
        private async Task PopulateAvailableRoles(CreateAccountViewModel model)
        {
            try
            {
                var rolesResponse = await _accountClient.GetAllRolesAsync(new MyProject.Grpc.Empty());
                model.AvailableRoles = new List<string>(rolesResponse.Roles);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi tải danh sách vai trò: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                // Thêm thông báo lỗi vào ModelState để hiển thị trên View nếu cần
                ModelState.AddModelError("", "Không thể tải danh sách vai trò.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi tải danh sách vai trò.");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tải danh sách vai trò.");
            }
        }
        private async Task PopulateAvailableRolesForEdit(EditAccountViewModel model)
        {
            try
            {
                var rolesResponse = await _accountClient.GetAllRolesAsync(new MyProject.Grpc.Empty());
                model.AvailableRoles = new List<string>(rolesResponse.Roles);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi tải danh sách vai trò cho Edit: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                ModelState.AddModelError("", "Không thể tải danh sách vai trò.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi tải danh sách vai trò cho Edit.");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tải danh sách vai trò.");
            }
        }
    }
}