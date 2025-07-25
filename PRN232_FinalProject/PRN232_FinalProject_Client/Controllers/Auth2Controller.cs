using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.DTO;
using PRN232_FinalProject_Client.JWTHelper;
using PRN232_FinalProject_Client.Services;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN232_FinalProject_Client.Controllers
{
    public class Auth2Controller : Controller
    {
        private readonly AuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Auth2Controller(AuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var tokenResponse = await _authService.LoginAsync(dto);

                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // Lấy FullName từ token
                    var fullName = JwtHelper.GetClaimFromToken(tokenResponse.Token, "FullName");
                    var userId = JwtHelper.GetClaimFromToken(tokenResponse.Token, ClaimTypes.NameIdentifier);
                    var role = JwtHelper.GetClaimFromToken(tokenResponse.Token, ClaimTypes.Role);
                    // Lưu vào ClaimsPrincipal
                    var claims = new List<Claim>
                {
                    new Claim("access_token", tokenResponse.Token)
                };
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        claims.Add(new Claim(ClaimTypes.Name, fullName));
                    }
                    if (!string.IsNullOrEmpty(role))
                    {
                        // 2. Thêm claim vai trò vào danh sách claims
                        // Đảm bảo giá trị của 'role' ở đây khớp với "Lecturer" hoặc "Staff" (case-sensitive)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                    if (!string.IsNullOrEmpty(userId))
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, userId)); // 👈 Thêm UserId vào claim
                    }
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
                    {
                        IsPersistent = false // Không lưu cookie sau khi đóng trình duyệt
                    });

                    // Lưu vào Session
                    _httpContextAccessor.HttpContext.Session.SetString("JWT", tokenResponse.Token);
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        _httpContextAccessor.HttpContext.Session.SetString("FullName", fullName);
                    }

                    return RedirectToAction("Index", "Article");
                }

                ModelState.AddModelError("", "Đăng nhập thất bại.");
                return View(dto);
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("Invalid credentials"))
                {
                    ModelState.AddModelError("Email", "Email không tồn tại hoặc mật khẩu không đúng.");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại sau.");
                }

                return View(dto);
            }
        }


        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _authService.RegisterAsync(dto);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Đăng ký thất bại. Email đã được sử dụng?");
                return View(dto);
            }

            return RedirectToAction("Login");
        }

        public IActionResult AccessDeny()
        {
            // Hiển thị thông báo truy cập bị từ chối
            ViewBag.ErrorMessage = "Bạn không có quyền truy cập vào trang này.";
            return View("AccessDenied", "Bạn không có quyền xem hồ sơ này.");
        }
        public async Task<IActionResult> Logout() // Đổi thành async Task<IActionResult>
        {
            // 1. Xóa Authentication Cookie (nơi chứa các claims)
            // Đây là bước QUAN TRỌNG NHẤT để đăng xuất người dùng
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Xóa các Session variables (nếu bạn vẫn muốn lưu trữ JWT và FullName trong Session)
            // Mặc dù đã chuyển sang dùng Claims, nếu bạn vẫn cần JWT/FullName trong Session vì lý do nào đó,
            // thì bước này vẫn có thể cần thiết. Tuy nhiên, nếu bạn đã dùng Claims hoàn toàn
            // cho việc hiển thị FullName trên UI như đã thảo luận, thì Session cho FullName là không cần nữa.
            _httpContextAccessor.HttpContext?.Session.Remove("JWT");
            _httpContextAccessor.HttpContext?.Session.Remove("FullName");

            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var result = await _authService.ForgotPasswordAsync(dto);
                if (result.Success)
                {
                    // Chuyển hướng đến trang đăng nhập khi thành công
                    return RedirectToAction("Login", "Auth2"); // Điều chỉnh tên action/controller nếu cần
                }
                else
                {
                    ModelState.AddModelError("Email", result.Message);
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi hệ thống khi gửi yêu cầu: {ex.Message}");
                return View(dto);
            }
        }



    }
}
