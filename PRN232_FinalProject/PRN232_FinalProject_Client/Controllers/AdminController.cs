// PRN232_FinalProject_Client/Controllers/AdminController.cs

using Grpc.Core; // Để bắt RpcException
using Microsoft.AspNetCore.Mvc;
using MyProject.Grpc; // Namespace cho AccountService (đã định nghĩa trong account.proto)
using PRN232_FinalProject_Client.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PRN232_FinalProject_Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly AccountService.AccountServiceClient _accountClient;
        private readonly ILogger<AdminController> _logger; // Nên có Logger để ghi log lỗi

        public AdminController(AccountService.AccountServiceClient accountClient, ILogger<AdminController> logger)
        {
            _accountClient = accountClient;
            _logger = logger;
        }

        // Action để hiển thị danh sách tất cả tài khoản
        public async Task<IActionResult> Index() // Đổi tên thành Index hoặc GetAccounts để dễ quản lý
        {
            try
            {
                // Gọi gRPC để lấy tất cả tài khoản
                var accountsList = await _accountClient.GetAllAccountsAsync(new Empty());

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
                var rolesResponse = await _accountClient.GetAllRolesAsync(new Empty());
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

            var model = new EditAccountViewModel(); // <-- DÙNG VIEWMODEL MỚI
            try
            {
                var request = new AccountIdRequest { UserId = id };
                // GetAccountByIdAsync trả về AccountRequest, cần ánh xạ nó sang EditAccountViewModel
                var accountGrpc = await _accountClient.GetAccountByIdAsync(request);

                // Ánh xạ AccountRequest sang EditAccountViewModel
                model.UserId = accountGrpc.UserId;
                model.Username = accountGrpc.Username;
                model.FullName = accountGrpc.FullName;
                model.Email = accountGrpc.Email;
                model.Role = accountGrpc.Role;
                

                await PopulateAvailableRolesForEdit(model); // <-- Tải danh sách vai trò
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
        public async Task<IActionResult> Edit(EditAccountViewModel model) // <-- NHẬN VIEWMODEL MỚI
        {
            //if (!ModelState.IsValid)
            //{
            //    ModelState.AddModelError("", "Cập nhật tài khoản thất bại.");
            //    await PopulateAvailableRolesForEdit(model); // <-- Tải lại danh sách vai trò nếu validation thất bại
            //    return View(model);
            //}

            try
            {
                var request = new AccountIdRequest { UserId = model.UserId };
                // GetAccountByIdAsync trả về AccountRequest, cần ánh xạ nó sang EditAccountViewModel
                var accountGrpc = await _accountClient.GetAccountByIdAsync(request);
                // Ánh xạ EditAccountViewModel sang AccountRequest để gửi qua gRPC
                if (model.NewPassword == null)
                {
                    var accountRequest1 = new AccountRequest
                    {
                        UserId = model.UserId,
                        Username = model.Username,
                        FullName = model.FullName,
                        Email = model.Email,
                        Role = model.Role,
                         Password = accountGrpc.Password, // Giữ nguyên mật khẩu cũ nếu không thay đổi
                    };

                    var reply1 = await _accountClient.UpdateAccountAsync(accountRequest1);
                    if (reply1 != null && !string.IsNullOrEmpty(reply1.Id)) // Sửa reply.Id thành reply.UserId
                    {
                        TempData["SuccessMessage"] = $"Tài khoản {reply1.Id} đã được cập nhật thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Cập nhật tài khoản thất bại.");
                    }
                }
                else {

                    var accountRequest = new AccountRequest
                    {
                        UserId = model.UserId,
                        Username = model.Username,
                        FullName = model.FullName,
                        Email = model.Email,
                        Role = model.Role,
                        Password = model.NewPassword ?? "", // Dùng NewPassword, nếu null thì gửi rỗng

                    };

                    var reply = await _accountClient.UpdateAccountAsync(accountRequest);
                    if (reply != null && !string.IsNullOrEmpty(reply.Id)) // Sửa reply.Id thành reply.UserId
                    {
                        TempData["SuccessMessage"] = $"Tài khoản {reply.Id} đã được cập nhật thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Cập nhật tài khoản thất bại.");
                    }
                }
                
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Lỗi gRPC khi cập nhật tài khoản: {StatusCode} - {Detail}", ex.StatusCode, ex.Status.Detail);
                ModelState.AddModelError("", $"Lỗi khi cập nhật tài khoản: {ex.Status.Detail}");
                await PopulateAvailableRolesForEdit(model); // <-- Tải lại danh sách vai trò sau lỗi gRPC
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi chung khi cập nhật tài khoản.");
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                await PopulateAvailableRolesForEdit(model); // <-- Tải lại danh sách vai trò sau lỗi chung
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
                var rolesResponse = await _accountClient.GetAllRolesAsync(new Empty());
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
                var rolesResponse = await _accountClient.GetAllRolesAsync(new Empty());
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