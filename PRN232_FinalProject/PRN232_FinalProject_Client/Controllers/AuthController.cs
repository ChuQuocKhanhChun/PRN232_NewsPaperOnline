using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.DTO;
using PRN232_FinalProject_Client.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PRN232_FinalProject_Client.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var tokenResponse = await _authService.LoginAsync(dto);
            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
            {
                HttpContext.Session.SetString("JWT", tokenResponse.Token);
                return RedirectToAction("Index", "Article");
            }
            ModelState.AddModelError("", "Đăng nhập thất bại");
            return View(dto);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var result = await _authService.RegisterAsync(dto);
            if (result)
                return RedirectToAction("Login");
            ModelState.AddModelError("", "Đăng ký thất bại");
            return View(dto);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWT");
            return RedirectToAction("Login");
        }
    }
}
