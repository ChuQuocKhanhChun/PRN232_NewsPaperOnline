using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRN232_FinalProject.Identity;

namespace PRN232_FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public RolesController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromQuery] string email, [FromQuery] string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded ? Ok("Role assigned") : BadRequest(result.Errors);
        }
    }
}
