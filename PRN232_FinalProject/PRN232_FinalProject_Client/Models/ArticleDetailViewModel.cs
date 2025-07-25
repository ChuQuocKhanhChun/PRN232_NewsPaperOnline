using PRN232_FinalProject_Client.DTO;

namespace PRN232_FinalProject_Client.Models
{
    public class ArticleDetailViewModel
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string AuthorName { get; set; } = string.Empty;

        // Danh sách bình luận đã được cấu trúc cây từ API
        
    }
}
