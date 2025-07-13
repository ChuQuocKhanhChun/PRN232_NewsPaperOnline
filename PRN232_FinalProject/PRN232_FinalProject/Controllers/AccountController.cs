using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject.Identity;

namespace PRN232_FinalProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> CreateTestUser()
        {
            var user = new ApplicationUser
            {
                UserName = "editor1",
                Email = "editor1@example.com",
                FullName = "Editor One"
            };

            var result = await _userManager.CreateAsync(user, "Password123@");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Editor");
                return Ok("User created");
            }

            return BadRequest(result.Errors);
        }
    }
}
