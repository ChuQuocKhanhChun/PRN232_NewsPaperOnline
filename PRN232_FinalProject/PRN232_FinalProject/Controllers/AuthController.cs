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
        private readonly ICloudinaryService _cloudinaryService;

        public AuthController(IAuthService authService,
                              SignInManager<ApplicationUser> signInManager,
                              IEmailService emailService,
                              UserManager<ApplicationUser> userManager, IUserService userService, ICloudinaryService cloudinaryService)
        {
            _authService = authService;
            _signInManager = signInManager;
            _emailService = emailService;
            _userManager = userManager;
            _user2Service = userService;
            _cloudinaryService = cloudinaryService;
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var profile = new UserProfileDto
            {
                Email = user.Email,
                FullName = user.FullName, // Ensure ApplicationUser has this property
                Roles = roles.ToList(),
                Image = user.Image, // Ensure ApplicationUser has this property
            };

            return Ok(profile);
        }
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Upload image if present
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);
                user.Image = imageUrl;
            }

            user.FullName = dto.FullName ?? user.FullName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "Profile updated", image = user.Image });
        }
        public class UpdateUserProfileDto
        {
            public string FullName { get; set; }
            public IFormFile? ImageFile { get; set; }
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest("Email không tồn tại trong hệ thống.");
            }

            // 1. Sinh mật khẩu random
            var newPassword = GenerateRandomPassword(10); // 10 ký tự
            Console.WriteLine($"Mật khẩu mới: {newPassword}");

            // 2. Tạo token và đổi mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            Console.WriteLine($"Token: {token}");
            var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!resetResult.Succeeded)
            {
                return BadRequest("Không thể đặt lại mật khẩu. Vui lòng thử lại sau.");
            }

            // 3. Gửi mật khẩu mới qua email
            var emailBody = $"Mật khẩu mới của bạn là: <b>{newPassword}</b>. Vui lòng đăng nhập và đổi lại mật khẩu!";
            await _emailService.SendEmailAsync(dto.Email, "Mật khẩu mới", emailBody);

            return Ok("Mật khẩu mới đã được gửi tới email của bạn.");
        }

        // Hàm sinh mật khẩu random
        private string GenerateRandomPassword(int length)
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*";
            const string allChars = lower + upper + digits + special;

            var random = new Random();
            var password = new StringBuilder();

            // Đảm bảo có ít nhất 1 ký tự từ mỗi loại
            password.Append(lower[random.Next(lower.Length)]);
            password.Append(upper[random.Next(upper.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Điền các ký tự còn lại
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Xáo trộn mật khẩu
            return new string(password.ToString().OrderBy(_ => random.Next()).ToArray());
        }


       
    }
}
