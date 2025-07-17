using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.DTO;
using PRN232_FinalProject_Client.JWTHelper;
using PRN232_FinalProject_Client.Services;
using System.Net.Http;
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
                    _httpContextAccessor.HttpContext.Session.SetString("JWT", tokenResponse.Token);

                    var fullName = JwtHelper.GetClaimFromToken(tokenResponse.Token, "FullName");
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


        public IActionResult Logout()
        {
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
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var result = await _authService.ForgotPasswordAsync(dto);
                if (result.Success) // Accessing the 'Success' property explicitly
                {
                    ViewBag.Message = result.Message; // Accessing the 'Message' property explicitly
                }
                else
                {
                    ModelState.AddModelError("Email", result.Message);
                }
            }
            catch
            {
                ModelState.AddModelError("", "Lỗi hệ thống khi gửi yêu cầu. Thử lại sau.");
            }

            return View(dto);
        }
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ResetPasswordDto
            {
                Email = email,
                Token = token
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Accessing the 'Success' property explicitly
            var result = await _authService.ResetPasswordAsync(model);

            if (result.Success) // Fixing the implicit conversion issue
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            ModelState.AddModelError("", "Đặt lại mật khẩu thất bại.");
            return View(model);
        }


    }
}
