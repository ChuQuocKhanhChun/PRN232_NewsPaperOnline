using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.Models
{
    public class EditAccountViewModel
    {
        [Required] // UserId là bắt buộc để xác định tài khoản
        public string UserId { get; set; }



        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Vai trò")]
        public string? Role { get; set; } // Role có thể là null nếu không chọn

        public List<string> AvailableRoles { get; set; } = new List<string>();

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu phải dài ít nhất {2} ký tự.", MinimumLength = 6)]
        [Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
        public string? NewPassword { get; set; }
    }
        
}
