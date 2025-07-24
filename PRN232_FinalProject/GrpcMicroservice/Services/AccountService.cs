using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyProject.Grpc;
using PRN232_FinalProject.Identity;
using PRN232_FinalProject.Models;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;

namespace GrpcMicroservice.Services
{
    public class AccountService : MyProject.Grpc.AccountService.AccountServiceBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) // <-- Cập nhật Constructor
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager)); // <-- Khởi tạo RoleManager
        }

        public override async Task<AccountReply> CreateAccount(AccountRequest request, ServerCallContext context)
        {
            var existingUser = await _userManager.FindByIdAsync(request.UserId);
            if (existingUser != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, $"Tài khoản với UserId '{request.UserId}' đã tồn tại."));
            }

            var user = new ApplicationUser
            {
                Id = request.UserId,
                UserName = request.Username, // <-- SỬ DỤNG TRƯỜNG USERNAME MỚI TỪ REQUEST
                Email = request.Email,
                FullName = request.FullName, // <-- Nếu ApplicationUser của bạn có trường FullName riêng
                IsActive = true, // Mặc định là active khi tạo
                Image = request.ImageUrl // Gán ImageUrl nếu có
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // ... (logic thêm vai trò) ...
                return new AccountReply
                {
                    Id = user.Id,
                    FullName = user.FullName, // <-- ĐIỀN DỮ LIỆU TỪ APPLICATIONUSER
                    Email = user.Email,       // <-- ĐIỀN DỮ LIỆU TỪ APPLICATIONUSER
                    IsActive = user.IsActive, // <-- ĐIỀN DỮ LIỆU TỪ APPLICATIONUSER
                    Image = user.Image ?? "/images/default_user.png"
                };
            }
            else
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Failed to create account: {errors}"));
            }
        }
        public override async Task<RoleList> GetAllRoles(Empty request, ServerCallContext context)
        {
            var list = new RoleList();
            try
            {
                // Lấy tất cả các vai trò từ RoleManager
                var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                list.Roles.AddRange(roles);
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Lỗi khi lấy danh sách vai trò: {ex.Message}"));
            }
            return list;
        }

        public override async Task<AccountReply> UpdateAccount(AccountRequest request, ServerCallContext context)
        {
            // request.UserId đã là string, không cần ToString()
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                // Ném RpcException khi không tìm thấy user
                throw new RpcException(new Status(StatusCode.NotFound, $"Tài khoản với UserId '{request.UserId}' không tìm thấy."));
            }

            // Cập nhật các thuộc tính của ApplicationUser từ request
            // Đảm bảo ApplicationUser có các thuộc tính này (FullName, ImageUrl, IsActive)
            user.UserName = request.Username; // Cập nhật UserName bằng Username từ request
            user.Email = request.Email;
            user.FullName = request.FullName; // Cập nhật FullName
            

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Cập nhật vai trò (nếu có sự thay đổi)
                // Logic này có thể phức tạp hơn nếu bạn muốn xử lý nhiều vai trò
                // hoặc xóa/thêm vai trò một cách chi tiết.
                // Ở đây, tôi giả định bạn chỉ muốn đặt lại một vai trò duy nhất.
                if (!string.IsNullOrEmpty(request.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (!currentRoles.Contains(request.Role))
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        await _userManager.AddToRoleAsync(user, request.Role);
                    }
                }

                // Cập nhật mật khẩu nếu request.Password không rỗng
                if (!string.IsNullOrEmpty(request.Password))
                {
                    // Quan trọng: Để cập nhật mật khẩu, bạn không cần mật khẩu cũ của người dùng
                    // trong trường hợp này (admin đặt lại). Bạn cần token đặt lại mật khẩu.
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);
                    if (!passwordResult.Succeeded)
                    {
                        var passwordErrors = string.Join("; ", passwordResult.Errors.Select(e => e.Description));
                        // Có thể ghi log lỗi mật khẩu và vẫn trả về thành công nếu các trường khác cập nhật được
                        // hoặc ném lỗi nếu muốn cập nhật mật khẩu là bắt buộc
                        throw new RpcException(new Status(StatusCode.Internal, $"Cập nhật tài khoản thành công nhưng mật khẩu thất bại: {passwordErrors}"));
                    }
                }

                // Trả về AccountReply với dữ liệu đã được cập nhật
                return new AccountReply
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Image = user.Image ?? "/images/default_user.png",
                    // Bỏ Message nếu đã xóa khỏi proto, hoặc để rỗng
                    // Message = "Account updated successfully."
                };
            }
            else
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                // Ném RpcException khi cập nhật thất bại
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Failed to update account: {errors}"));
            }
        }

        public override async Task<AccountReply> ToggleAccountActive(AccountIdRequest request, ServerCallContext context)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Tài khoản với UserId '{request.UserId}' không tìm thấy."));
            }

            // Đảo ngược trạng thái IsActive hiện tại
            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new RpcException(new Status(StatusCode.Internal, $"Failed to toggle account status: {errors}"));
            }

            // Trả về AccountReply với trạng thái mới nhất
            return new AccountReply
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive, // <-- TRẠNG THÁI ĐÃ ĐƯỢC CẬP NHẬT
                Image = user.Image ?? "/images/default_user.png"
            };
        }

        // SỬA PHƯƠNG THỨC GetAllAccounts
        public override async Task<AccountList> GetAllAccounts(Empty request, ServerCallContext context)
        {
            var list = new AccountList();
            try
            {
                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    list.Accounts.Add(new AccountReply
                    {
                        Id = user.Id,
                        FullName = user.FullName,         // Điền FullName
                        Email = user.Email,               // Điền Email
                        IsActive = user.IsActive,         // Điền IsActive
                        Image = user.Image ?? "/images/default_user.png" // Điền ImageUrl, dùng default nếu null
                        // Bỏ trường Message nếu đã xóa khỏi proto
                    });
                }
            }
            catch (Exception ex)
            {
                // Ném RpcException khi có lỗi
                throw new RpcException(new Status(StatusCode.Internal, $"Lỗi trong GetAllAccounts: {ex.Message}"));
            }
            return list;
        }



        public override async Task<AccountRequest> GetAccountById(AccountIdRequest request, ServerCallContext context)
            {
                // Kiểm tra xem ID có hợp lệ không
                if (string.IsNullOrEmpty(request.UserId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId không được để trống."));
                }

                // Sử dụng UserManager để tìm người dùng theo ID
                var user = await _userManager.FindByIdAsync(request.UserId);

                if (user == null)
                {
                    // Nếu không tìm thấy người dùng, ném RpcException với StatusCode.NotFound
                    throw new RpcException(new Status(StatusCode.NotFound, $"Tài khoản với UserId '{request.UserId}' không tìm thấy."));
                }

                // Lấy vai trò của người dùng (nếu có)
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "None"; // Giả sử chỉ lấy vai trò đầu tiên hoặc "None"

                // Ánh xạ từ ApplicationUser sang AccountRequest
                // RẤT QUAN TRỌNG: KHÔNG BAO GIỜ TRẢ VỀ MẬT KHẨU THẬT QUA GỬI TIN NHẮN
                // Trường 'Password' trong AccountRequest proto có thể được sử dụng cho mục đích tạo/cập nhật mật khẩu mới,
                // nhưng khi đọc, nó nên được bỏ qua hoặc đặt thành chuỗi rỗng.
                return new AccountRequest
                {
                    UserId = user.Id,
                    FullName = user.UserName, // Giả sử UserName dùng làm FullName hoặc bạn có một thuộc tính FullName riêng
                    Email = user.Email,
                    Role = role,
                    Password = "" // Luôn để trống hoặc null khi trả về thông tin account để tránh rò rỉ mật khẩu
                };
            }

        }
    
}