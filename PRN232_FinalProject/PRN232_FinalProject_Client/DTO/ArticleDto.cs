using PRN232_FinalProject.Identity;

namespace PRN232_FinalProject_Client.DTO
{
    public class ArticleDto
    {
        public int ArticleID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; } 
        public string Status { get; set; } // Published, Draft, Archived, Pending
        public int? ViewCount { get; set; } = 0; // Default to 0
        public DateTime? CreatedAt { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string ? ImageUrl { get; set; } // Optional, for article images
        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // optional, useful for display    

        public List<int> TagIds { get; set; } = new();
        public List<string> TagNames { get; set; } = new();
        public int LikesCount { get; set; } = 0; // Số lượt thích, mặc định là 0
    }
}
