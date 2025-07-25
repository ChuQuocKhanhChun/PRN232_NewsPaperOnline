using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject_Client.DTO;
using PRN232_FinalProject_Client.Services;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace PRN232_FinalProject_Client.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(UserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpPost]
        
        public async Task<IActionResult> UpdateProfile(UpdateUserProfileDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Chuẩn bị content để gọi API (multipart/form-data)
            using var content = new MultipartFormDataContent();

            if (!string.IsNullOrEmpty(model.FullName))
                content.Add(new StringContent(model.FullName), "FullName");

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var streamContent = new StreamContent(model.ImageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.ImageFile.ContentType);
                content.Add(streamContent, "ImageFile", model.ImageFile.FileName);
            }

            var response = await _userService.CallUpdateProfileApiAsync(userId, content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Cập nhật thất bại!";
                return RedirectToAction("Profile");
            }

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Profile");
        }





        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Thử lấy token từ ClaimsPrincipal
            var token = HttpContext.User.FindFirst("access_token")?.Value;
            Console.WriteLine($"Token từ Claims: {token}");

            // Fallback về Session nếu cần
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext.Session.GetString("JWT");
                Console.WriteLine($"Token từ Session: {token}");
            }

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth2");
            }

            try
            {
                var userProfile = await _userService.GetUserProfileAsync(token);
                if (userProfile == null)
                {
                    ViewBag.ErrorMessage = "Không thể lấy thông tin cá nhân. Vui lòng thử lại.";
                    return View("Error");
                }
               
                    TempData["Error"] = null;
                

                TempData["Success"] = null;
                return View("Profile", userProfile);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi hệ thống: {ex.Message}";
                return View("Error");
            }
        }

    }
}
