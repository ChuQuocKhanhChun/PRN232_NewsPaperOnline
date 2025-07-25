using System.ComponentModel.DataAnnotations;

namespace PRN232_FinalProject_Client.Models
{
    public class EditTagViewModel
    {
        public int TagId { get; set; }

        [Required(ErrorMessage = "Tên Tag là bắt buộc.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên Tag phải dài từ 2 đến 100 ký tự.")]
        public string Name { get; set; } = string.Empty;
    }
}
