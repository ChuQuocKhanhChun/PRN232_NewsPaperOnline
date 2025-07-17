using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.DTO
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        // Không cần bind từ form, set mặc định phía controller
        public string Role { get; set; } = "Reader";
    }


}