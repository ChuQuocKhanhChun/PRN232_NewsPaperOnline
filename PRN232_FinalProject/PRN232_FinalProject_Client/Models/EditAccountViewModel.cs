using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.Models
{
    public class EditAccountViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } // Giả định bạn đã thêm Username vào AccountRequest

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Vai trò")]
        public string Role { get; set; }

        // Danh sách các vai trò có sẵn để hiển thị trong dropdown
        public List<string> AvailableRoles { get; set; } = new List<string>();

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Mật khẩu phải dài ít nhất {2} ký tự.", MinimumLength = 6)]
        [Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
        public string NewPassword { get; set; } // Dùng cho việc thay đổi mật khẩu

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; }

        [Display(Name = "Đường dẫn hình ảnh")]
        [Url(ErrorMessage = "Đường dẫn hình ảnh không hợp lệ.")]
        public string ImageUrl { get; set; }
    }
}
