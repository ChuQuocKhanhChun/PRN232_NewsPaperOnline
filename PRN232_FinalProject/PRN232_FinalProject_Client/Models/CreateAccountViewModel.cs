using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.Models
{
    public class CreateAccountViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải dài ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")] 
        [Display(Name = "Tên đăng nhập")] 
        public string Username { get; set; } 

        public List<string> AvailableRoles { get; set; } = new List<string>();
    }
}
