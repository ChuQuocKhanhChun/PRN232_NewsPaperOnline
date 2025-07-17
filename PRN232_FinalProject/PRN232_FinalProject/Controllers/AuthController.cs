using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PRN232_FinalProject.DTO;
using PRN232_FinalProject.Identity;
using PRN232_FinalProject.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PRN232_FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {   private readonly IUserService _user2Service;
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authService,
                              SignInManager<ApplicationUser> signInManager,
                              IEmailService emailService,
                              UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _authService = authService;
            _signInManager = signInManager;
            _emailService = emailService;
            _userManager = userManager;
            _user2Service = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return result ? Ok("User registered") : BadRequest("Failed to register");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                return BadRequest("Login failed.");
            }

            return Ok(new { token = result }); // <-- Trả 200 OK đúng cách
        }


        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logged out");
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized("Missing email claim");

            var profile = await _authService.GetProfileAsync(email);
            return profile == null ? NotFound("User not found") : Ok(profile);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest("Email không tồn tại trong hệ thống.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);

            var resetLink = $"https://localhost:7030/Auth2/ResetPassword?email={dto.Email}&token={encodedToken}";


            // Gửi email (ví dụ: thông qua SmtpClient hoặc service gửi mail)
            await _emailService.SendEmailAsync(dto.Email, "Đặt lại mật khẩu", $"Click vào liên kết sau để đặt lại mật khẩu: <a href=\"{resetLink}\">Reset Password</a>");

            return Ok("Email đặt lại mật khẩu đã được gửi.");
        }


        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await _user2Service.ResetPasswordAsync(dto);
            if (!success) return BadRequest("Không thể đặt lại mật khẩu. Token sai hoặc email không đúng.");

            return Ok("Đặt lại mật khẩu thành công.");
        }
    }
}
