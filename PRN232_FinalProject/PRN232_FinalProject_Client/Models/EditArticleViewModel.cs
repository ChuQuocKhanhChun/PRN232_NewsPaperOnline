using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.Models
{
    public class EditArticleViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc.")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Ảnh bài báo")] // Đổi tên hiển thị
        [DataType(DataType.Upload)] // Gợi ý cho front-end
        public IFormFile? ImageFile { get; set; } // <-- THAY THẾ ImageUrl bằng ImageFile
        public string? ImageUrl { get; set; }

        [Display(Name = "ID Tác giả")]
        public string AuthorId { get; set; } = string.Empty; // Readonly

        [Display(Name = "Ngày tạo")]
        public string CreatedAt { get; set; } = string.Empty; // Readonly

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Từ khóa")]
        public List<int>? SelectedTagIds { get; set; } // <-- THAY THẾ TagsString BẰNG LIST CÁC ID TAG ĐƯỢC CHỌN
        public MultiSelectList? AvailableTags { get; set; } // <-- DANH SÁCH TAGS ĐỂ CHỌN

        public IEnumerable<SelectListItem>? Categories { get; set; } // Để đổ dữ liệu vào DropdownList
    }
}
