using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.Models
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Tên Category là bắt buộc.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên Category phải dài từ 2 đến 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}
