using GrpcArticleService;

namespace PRN232_FinalProject_Client.Models
{
    public class ArticleListViewModel
    {
        public List<ArticleResponse> Articles { get; set; } = new List<ArticleResponse>();
        // Có thể thêm các thuộc tính cho phân trang, tìm kiếm nếu cần
    }
}
